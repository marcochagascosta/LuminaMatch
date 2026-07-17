using System;
using LuminaMatch.Match3;
using UnityEngine;
using UnityEngine.UI;

namespace LuminaMatch.UI
{
    /// <summary>Renders the match board with Royal Match art and raises cell clicks.</summary>
    public class BoardPresenter : MonoBehaviour
    {
        public event Action<int, int> CellClicked;

        RectTransform _gridRoot;
        Image[,] _gemImages;
        Image[,] _iceOverlays;
        Image[,] _powerOverlays;
        BoardModel _board;
        float _cellSize;
        Sprite _white;

        public void Bind(RectTransform parent, BoardModel board, Vector2 center, float size, Sprite whiteSprite)
        {
            _board = board;
            _white = whiteSprite;
            _cellSize = size / Mathf.Max(board.Width, board.Height);

            ClearChildren(parent);

            var bg = MakeImage(parent, "BoardBg", ArtCatalog.Background);
            bg.rectTransform.sizeDelta = new Vector2(size + 120, size + 120);
            bg.rectTransform.anchoredPosition = center;
            bg.preserveAspect = true;

            var frame = MakeImage(parent, "BoardFrame", ArtCatalog.Frame);
            frame.rectTransform.sizeDelta = new Vector2(size + 40, size + 40);
            frame.rectTransform.anchoredPosition = center;
            frame.preserveAspect = true;
            frame.raycastTarget = false;

            var gridGo = new GameObject("Board", typeof(RectTransform));
            gridGo.transform.SetParent(parent, false);
            _gridRoot = gridGo.GetComponent<RectTransform>();
            _gridRoot.sizeDelta = new Vector2(size, size);
            _gridRoot.anchoredPosition = center;

            int w = board.Width;
            int h = board.Height;
            _gemImages = new Image[w, h];
            _iceOverlays = new Image[w, h];
            _powerOverlays = new Image[w, h];

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int bx = x, by = y;
                var cellGo = new GameObject($"C{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                cellGo.transform.SetParent(_gridRoot, false);
                var rt = cellGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_cellSize - 4, _cellSize - 4);
                rt.anchoredPosition = CellPos(x, y, w, h);

                var img = cellGo.GetComponent<Image>();
                img.preserveAspect = true;
                _gemImages[x, y] = img;
                cellGo.GetComponent<Button>().onClick.AddListener(() => CellClicked?.Invoke(bx, by));

                var iceGo = new GameObject("Ice", typeof(RectTransform), typeof(Image));
                iceGo.transform.SetParent(cellGo.transform, false);
                Stretch(iceGo.GetComponent<RectTransform>());
                var iceImg = iceGo.GetComponent<Image>();
                iceImg.sprite = ArtCatalog.Ice;
                iceImg.preserveAspect = true;
                iceImg.raycastTarget = false;
                _iceOverlays[x, y] = iceImg;

                var pGo = new GameObject("Power", typeof(RectTransform), typeof(Image));
                pGo.transform.SetParent(cellGo.transform, false);
                var prt = pGo.GetComponent<RectTransform>();
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.sizeDelta = new Vector2(_cellSize * 0.42f, _cellSize * 0.42f);
                prt.anchoredPosition = new Vector2(_cellSize * 0.2f, _cellSize * 0.2f);
                var pImg = pGo.GetComponent<Image>();
                pImg.preserveAspect = true;
                pImg.raycastTarget = false;
                _powerOverlays[x, y] = pImg;
            }

            Refresh(null);
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
                if (cell.IsHole)
                {
                    img.sprite = _white;
                    img.color = new Color(0.1f, 0.1f, 0.1f, 0.25f);
                    _iceOverlays[x, y].enabled = false;
                    _powerOverlays[x, y].enabled = false;
                    continue;
                }

                if (cell.Blocker == BlockerType.Box)
                {
                    img.sprite = ArtCatalog.Box;
                    img.color = Color.white;
                    _iceOverlays[x, y].enabled = false;
                    _powerOverlays[x, y].enabled = false;
                }
                else
                {
                    img.sprite = ArtCatalog.Gem(cell.Color);
                    img.color = Color.white;
                    bool iced = cell.Blocker == BlockerType.Ice;
                    _iceOverlays[x, y].enabled = iced;
                    _iceOverlays[x, y].color = new Color(1f, 1f, 1f, 0.85f);

                    bool powered = cell.HasPower;
                    _powerOverlays[x, y].enabled = powered;
                    if (powered)
                    {
                        _powerOverlays[x, y].sprite = ArtCatalog.Power(cell.Power);
                        _powerOverlays[x, y].color = Color.white;
                    }
                }

                if (selected.HasValue && selected.Value.x == x && selected.Value.y == y)
                    img.color = Color.Lerp(img.color, Color.white, 0.35f);
            }
        }

        public RectTransform CellRect(int x, int y)
            => _gemImages != null ? _gemImages[x, y].rectTransform : null;

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
                if (c.name.StartsWith("Board") || c.name == "BoardBg" || c.name == "BoardFrame")
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
