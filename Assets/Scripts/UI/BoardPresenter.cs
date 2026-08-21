using System;
using LuminaMatch.Match3;
using UnityEngine;
using UnityEngine.UI;

namespace LuminaMatch.UI
{
    /// <summary>Renders the match board with Royal Match art; supports drag-to-swap.</summary>
    public class BoardPresenter : MonoBehaviour
    {
        public event Action<int, int> CellClicked;
        public event Action<int, int, int, int> SwapRequested;

        RectTransform _gridRoot;
        Image[,] _gemImages;
        Image[,] _iceOverlays;
        Image[,] _powerOverlays;
        BoardModel _board;
        float _cellSize;
        Sprite _white;
        bool _inputLocked;

        public float CellSize => _cellSize;
        public RectTransform GridRoot => _gridRoot;

        public void SetInputLocked(bool locked) => _inputLocked = locked;

        // board_frame_ring.png: 1059×965 with 28px transparent pad, content ≈ 1003×909,
        // alpha hole ≈ 763×669 (mid-cross). No MaxOuterFrame — pad may hang off-screen so
        // the visible gold/crystal content can fill the phone.
        const float SpriteW = 1059f;
        const float SpriteH = 965f;
        const float ContentW = 1003f;
        const float ContentH = 909f;
        const float HoleW = 763f;
        const float HoleH = 669f;
        /// <summary>Grow board slightly past the inscribed square so gems eat the wide-hole letterbox into the ornate lip (still inside the gold).</summary>
        const float HoleOverscan = 1.06f;
        const float InnerPad = 2f;
        const float PanelSideExtra = 4f;
        const float GemScale = 0.92f;
        const float GemPoweredScale = 0.60f;
        const float PowerScale = 0.98f;
        const float BoxScale = 1.0f;
        const float IceScale = 0.82f;
        const float CellInset = 2f;

        /// <summary>
        /// Largest square board whose ornate frame content (gold + crystals) fits inside
        /// maxVisibleW × maxVisibleH. Transparent sprite padding may extend past that box.
        /// </summary>
        public static float MaxBoardSizeForVisible(float maxVisibleW, float maxVisibleH)
        {
            float spriteW = SpriteW, spriteH = SpriteH, contentW = ContentW, contentH = ContentH;
            float holeW = HoleW, holeH = HoleH;
            if (ArtCatalog.Frame != null && ArtCatalog.Frame.rect.height > 1f)
            {
                float sx = ArtCatalog.Frame.rect.width / SpriteW;
                float sy = ArtCatalog.Frame.rect.height / SpriteH;
                spriteW = ArtCatalog.Frame.rect.width;
                spriteH = ArtCatalog.Frame.rect.height;
                contentW = ContentW * sx;
                contentH = ContentH * sy;
                holeW = HoleW * sx;
                holeH = HoleH * sy;
            }

            // Content must stay on-screen; pad may clip.
            float frameW = Mathf.Min(
                maxVisibleW * spriteW / contentW,
                maxVisibleH * spriteW / contentH);
            float innerRatio = Mathf.Min(holeW, holeH) / spriteW * HoleOverscan;
            return Mathf.Max(120f, frameW * innerRatio - InnerPad);
        }

        public static Vector2 FrameSizeForBoard(float boardSize)
        {
            float spriteW = SpriteW, spriteH = SpriteH;
            float holeW = HoleW, holeH = HoleH;
            if (ArtCatalog.Frame != null && ArtCatalog.Frame.rect.height > 1f)
            {
                spriteW = ArtCatalog.Frame.rect.width;
                spriteH = ArtCatalog.Frame.rect.height;
                holeW = HoleW * (spriteW / SpriteW);
                holeH = HoleH * (spriteH / SpriteH);
            }

            // Inverse of MaxBoardSizeForVisible: board ≈ holeMin * overscan.
            float innerRatio = Mathf.Min(holeW, holeH) / spriteW * HoleOverscan;
            float frameW = (boardSize + InnerPad) / innerRatio;
            float frameH = frameW * (spriteH / spriteW);
            return new Vector2(frameW, frameH);
        }

        /// <summary>Visible gold+crystal content size for a given board (excludes transparent pad).</summary>
        public static Vector2 ContentSizeForBoard(float boardSize)
        {
            Vector2 frame = FrameSizeForBoard(boardSize);
            return new Vector2(frame.x * ContentW / SpriteW, frame.y * ContentH / SpriteH);
        }

        public void Bind(RectTransform parent, BoardModel board, Vector2 center, float size, Sprite whiteSprite)
        {
            _board = board;
            _white = whiteSprite;
            _inputLocked = false;

            ClearChildren(parent);

            float boardSize = size;
            Vector2 frameSize = FrameSizeForBoard(boardSize);
            float frameWidth = frameSize.x;
            float frameHeight = frameSize.y;
            _cellSize = boardSize / Mathf.Max(board.Width, board.Height);

            // Backdrop under the gem hole only (not a tall empty slab).
            var panel = MakeImage(parent, "BoardPanel", whiteSprite);
            panel.rectTransform.sizeDelta = new Vector2(
                boardSize + PanelSideExtra,
                boardSize + PanelSideExtra);
            panel.rectTransform.anchoredPosition = center;
            panel.color = new Color(0.02f, 0.03f, 0.1f, 0.55f);
            panel.raycastTarget = false;

            var frame = MakeImage(parent, "BoardFrame", ArtCatalog.Frame);
            frame.rectTransform.sizeDelta = new Vector2(frameWidth, frameHeight);
            frame.rectTransform.anchoredPosition = center;
            frame.preserveAspect = true;
            frame.raycastTarget = false;
            frame.color = Color.white;

            var gridGo = new GameObject("Board", typeof(RectTransform));
            gridGo.transform.SetParent(parent, false);
            _gridRoot = gridGo.GetComponent<RectTransform>();
            _gridRoot.sizeDelta = new Vector2(boardSize, boardSize);
            _gridRoot.anchoredPosition = center;
            frame.transform.SetSiblingIndex(panel.transform.GetSiblingIndex() + 1);

            int w = board.Width;
            int h = board.Height;
            _gemImages = new Image[w, h];
            _iceOverlays = new Image[w, h];
            _powerOverlays = new Image[w, h];

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int bx = x, by = y;
                var cellGo = new GameObject($"C{x}_{y}", typeof(RectTransform), typeof(Image), typeof(GemCellInput));
                cellGo.transform.SetParent(_gridRoot, false);
                var rt = cellGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_cellSize - CellInset, _cellSize - CellInset);
                rt.anchoredPosition = CellPos(x, y, w, h);

                // Fully transparent hit target (no plate behind gems).
                var cellBg = cellGo.GetComponent<Image>();
                cellBg.sprite = whiteSprite;
                cellBg.color = new Color(1f, 1f, 1f, 0f);
                cellBg.raycastTarget = true;

                var input = cellGo.GetComponent<GemCellInput>();
                input.X = bx;
                input.Y = by;
                input.Owner = this;

                var gemGo = new GameObject("Gem", typeof(RectTransform), typeof(Image));
                gemGo.transform.SetParent(cellGo.transform, false);
                var gemRt = gemGo.GetComponent<RectTransform>();
                gemRt.anchorMin = gemRt.anchorMax = new Vector2(0.5f, 0.5f);
                gemRt.sizeDelta = new Vector2(_cellSize * GemScale, _cellSize * GemScale);
                gemRt.anchoredPosition = Vector2.zero;
                var gemImg = gemGo.GetComponent<Image>();
                gemImg.preserveAspect = true;
                gemImg.raycastTarget = false;
                _gemImages[x, y] = gemImg;

                var iceGo = new GameObject("Ice", typeof(RectTransform), typeof(Image));
                iceGo.transform.SetParent(cellGo.transform, false);
                var iceRt = iceGo.GetComponent<RectTransform>();
                iceRt.anchorMin = iceRt.anchorMax = new Vector2(0.5f, 0.5f);
                // Inset frost overlay so it doesn't read as a solid white tile.
                iceRt.sizeDelta = new Vector2(_cellSize * IceScale, _cellSize * IceScale);
                iceRt.anchoredPosition = Vector2.zero;
                var iceImg = iceGo.GetComponent<Image>();
                iceImg.sprite = ArtCatalog.Ice;
                iceImg.preserveAspect = true;
                iceImg.raycastTarget = false;
                _iceOverlays[x, y] = iceImg;

                // Powers are the main piece visual (not a tiny corner badge).
                var pGo = new GameObject("Power", typeof(RectTransform), typeof(Image));
                pGo.transform.SetParent(cellGo.transform, false);
                var prt = pGo.GetComponent<RectTransform>();
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.sizeDelta = new Vector2(_cellSize * PowerScale, _cellSize * PowerScale);
                prt.anchoredPosition = Vector2.zero;
                var pImg = pGo.GetComponent<Image>();
                pImg.preserveAspect = true;
                pImg.raycastTarget = false;
                _powerOverlays[x, y] = pImg;
            }

            Refresh(null);
        }

        public void NotifyPointerDown(int x, int y)
        {
            // Reserved for future press feedback.
        }

        public void NotifyCellClicked(int x, int y)
        {
            if (_inputLocked) return;
            CellClicked?.Invoke(x, y);
        }

        public bool TryRequestDragSwap(int x1, int y1, int x2, int y2)
        {
            if (_inputLocked || _board == null) return false;
            if (!_board.InBounds(x1, y1) || !_board.InBounds(x2, y2)) return false;
            if (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) != 1) return false;
            SwapRequested?.Invoke(x1, y1, x2, y2);
            return true;
        }

        public void Refresh((int x, int y)? selected)
        {
            if (_board == null || _gemImages == null) return;
            int w = _board.Width, h = _board.Height;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var cell = _board.Grid[x, y];
                var img = _gemImages[x, y];
                var cellBg = img.transform.parent.GetComponent<Image>();
                var powerRt = _powerOverlays[x, y].rectTransform;

                if (cell.IsHole)
                {
                    img.enabled = false;
                    if (cellBg != null) cellBg.color = new Color(0.05f, 0.05f, 0.08f, 0.35f);
                    _iceOverlays[x, y].enabled = false;
                    _powerOverlays[x, y].enabled = false;
                    continue;
                }

                if (cell.Blocker == BlockerType.Box)
                {
                    img.enabled = true;
                    img.sprite = ArtCatalog.Box;
                    img.color = Color.white;
                    img.rectTransform.sizeDelta = new Vector2(_cellSize * BoxScale, _cellSize * BoxScale);
                    if (cellBg != null) cellBg.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
                    _iceOverlays[x, y].enabled = false;
                    _powerOverlays[x, y].enabled = false;
                }
                else if (cell.Color == GemColor.None)
                {
                    img.enabled = false;
                    _iceOverlays[x, y].enabled = false;
                    _powerOverlays[x, y].enabled = false;
                }
                else
                {
                    bool powered = cell.HasPower;
                    img.enabled = true;
                    img.sprite = ArtCatalog.Gem(cell.Color);
                    img.color = new Color(1f, 1f, 1f, powered ? 0.55f : 1f);
                    // When powered, gem shrinks behind the large power icon.
                    img.rectTransform.sizeDelta = powered
                        ? new Vector2(_cellSize * GemPoweredScale, _cellSize * GemPoweredScale)
                        : new Vector2(_cellSize * GemScale, _cellSize * GemScale);

                    bool iced = cell.Blocker == BlockerType.Ice;
                    _iceOverlays[x, y].enabled = iced;
                    if (iced)
                    {
                        _iceOverlays[x, y].color = new Color(0.8f, 0.94f, 1f, 0.72f);
                        _iceOverlays[x, y].rectTransform.sizeDelta = new Vector2(_cellSize * IceScale, _cellSize * IceScale);
                        _iceOverlays[x, y].rectTransform.SetAsLastSibling();
                    }

                    _powerOverlays[x, y].enabled = powered;
                    if (powered)
                    {
                        _powerOverlays[x, y].sprite = ArtCatalog.Power(cell.Power);
                        _powerOverlays[x, y].color = Color.white;
                        powerRt.sizeDelta = new Vector2(_cellSize * PowerScale, _cellSize * PowerScale);
                        powerRt.anchoredPosition = Vector2.zero;
                        powerRt.SetAsLastSibling();
                    }
                }

                if (selected.HasValue && selected.Value.x == x && selected.Value.y == y && img.enabled)
                    img.color = Color.Lerp(img.color, Color.white, 0.35f) * 1.15f;
            }
        }

        public RectTransform CellRect(int x, int y)
            => _gemImages != null ? _gemImages[x, y].rectTransform : null;

        public RectTransform CellRoot(int x, int y)
            => _gemImages != null ? _gemImages[x, y].transform.parent as RectTransform : null;

        Vector2 CellPos(int x, int y, int w, int h)
            => new((x - (w - 1) * 0.5f) * _cellSize, (y - (h - 1) * 0.5f) * _cellSize);

        static Image MakeImage(Transform parent, string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            return img;
        }

        static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i);
                var n = c.name;
                if (n == "Board" || n == "BoardPanel" || n == "BoardFrame" || n == "BoardBg")
                    UnityEngine.Object.Destroy(c.gameObject);
            }
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
