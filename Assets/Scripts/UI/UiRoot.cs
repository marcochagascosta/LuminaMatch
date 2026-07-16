using LuminaMatch.Audio;
using LuminaMatch.Economy;
using LuminaMatch.Match3;
using LuminaMatch.Meta;
using LuminaMatch.Monetization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LuminaMatch.UI
{
    public enum AppScreen
    {
        Home,
        LevelSelect,
        Gameplay,
        Result,
        Shop,
        OutOfLives
    }

    public class UiRoot : MonoBehaviour
    {
        Canvas _canvas;
        RectTransform _root;
        AppScreen _screen = AppScreen.Home;
        Match3Session _session;
        int _selectedLevel = 1;
        bool _lastWon;
        string _statusMessage = "";
        BoosterType? _pendingBooster;
        (int x, int y)? _selectedCell;

        Font _font;
        Sprite _whiteSprite;

        void Start()
        {
            try
            {
                EnsureEventSystem();
                _font = ResolveFont();
                _whiteSprite = CreateWhiteSprite();

                var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasGo.transform.SetParent(transform, false);
                _canvas = canvasGo.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                var rootGo = new GameObject("Root", typeof(RectTransform));
                rootGo.transform.SetParent(canvasGo.transform, false);
                _root = rootGo.GetComponent<RectTransform>();
                Stretch(_root);

                Show(AppScreen.Home);
                Debug.Log("[LuminaMatch] UiRoot started OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                // Keep process alive so device logs remain readable.
            }
        }

        static Font ResolveFont()
        {
            // Built-in Arial/LegacyRuntime often null or crashy on iOS/Android players.
            Font font = null;
#if UNITY_IOS && !UNITY_EDITOR
            font = Font.CreateDynamicFontFromOSFont(new[] { "Helvetica", "Helvetica Neue", "Arial" }, 32);
#elif UNITY_ANDROID && !UNITY_EDITOR
            font = Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "sans-serif", "Arial" }, 32);
#else
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "sans-serif" }, 32);
#endif
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            return font;
        }

        static Sprite CreateWhiteSprite()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
        }

        void Update()
        {
            PlayerProgress.Instance?.TickLives();
        }

        public void Show(AppScreen screen)
        {
            _screen = screen;
            Rebuild();
        }

        void Rebuild()
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);

            switch (_screen)
            {
                case AppScreen.Home: BuildHome(); break;
                case AppScreen.LevelSelect: BuildLevelSelect(); break;
                case AppScreen.Gameplay: BuildGameplay(); break;
                case AppScreen.Result: BuildResult(); break;
                case AppScreen.Shop: BuildShop(); break;
                case AppScreen.OutOfLives: BuildOutOfLives(); break;
            }
        }

        void BuildHome()
        {
            var p = PlayerProgress.Instance;
            AddBackground(new Color(0.08f, 0.07f, 0.16f));
            AddLabel("LUMINA MATCH", 64, new Vector2(0, 700), new Color(1f, 0.9f, 0.55f));
            AddLabel("Restaure o Palácio de Luz", 32, new Vector2(0, 600), Color.white);
            AddLabel(CastleProgress.StatusText(p), 28, new Vector2(0, 500), new Color(0.7f, 0.85f, 1f));
            AddLabel($"Moedas: {p.Data.Coins}   Vidas: {p.Data.Lives}/{p.Data.MaxLives}", 26, new Vector2(0, 400), Color.white);

            float regen = p.SecondsToNextLife();
            if (regen > 0)
                AddLabel($"Próxima vida em {regen / 60:00}:{regen % 60:00}", 22, new Vector2(0, 340), new Color(0.8f, 0.8f, 0.8f));

            if (!string.IsNullOrEmpty(_statusMessage))
                AddLabel(_statusMessage, 24, new Vector2(0, 260), new Color(1f, 0.75f, 0.4f));

            AddButton("Jogar", new Vector2(0, 80), () =>
            {
                _statusMessage = "";
                Show(AppScreen.LevelSelect);
            });
            AddButton("Loja", new Vector2(0, -60), () => Show(AppScreen.Shop));
            AddButton($"Continuar nível {p.Data.HighestUnlockedLevel}", new Vector2(0, -200), () => TryStartLevel(p.Data.HighestUnlockedLevel));
        }

        void BuildLevelSelect()
        {
            var p = PlayerProgress.Instance;
            AddBackground(new Color(0.07f, 0.09f, 0.18f));
            AddLabel("Mapa de Níveis", 48, new Vector2(0, 820), Color.white);
            AddButton("Voltar", new Vector2(-400, 820), () => Show(AppScreen.Home), new Vector2(180, 70));

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(_root, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0.08f);
            scrollRt.anchorMax = new Vector2(0.95f, 0.82f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scrollGo.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            for (int i = 1; i <= LevelCatalog.TotalLevels; i++)
            {
                int level = i;
                bool unlocked = LevelMap.IsUnlocked(p, level);
                var row = new GameObject($"L{level}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                row.transform.SetParent(content.transform, false);
                row.GetComponent<LayoutElement>().preferredHeight = 90;
                row.GetComponent<Image>().color = unlocked
                    ? new Color(0.25f, 0.35f, 0.55f)
                    : new Color(0.15f, 0.15f, 0.2f);
                var btn = row.GetComponent<Button>();
                btn.interactable = unlocked;
                int captured = level;
                btn.onClick.AddListener(() => TryStartLevel(captured));

                var label = CreateText(row.transform, unlocked ? $"Nível {level} — {LevelCatalog.Get(level).Title}" : $"Nível {level} [bloqueado]", 28);
                Stretch(label.rectTransform);
                label.alignment = TextAnchor.MiddleCenter;
            }
        }

        void TryStartLevel(int levelId)
        {
            var p = PlayerProgress.Instance;
            if (!LevelMap.IsUnlocked(p, levelId))
            {
                _statusMessage = "Nível bloqueado.";
                Show(AppScreen.Home);
                return;
            }

            if (!p.TrySpendLife())
            {
                Show(AppScreen.OutOfLives);
                return;
            }

            _selectedLevel = levelId;
            _session = new Match3Session(LevelCatalog.Get(levelId));
            _pendingBooster = null;
            _selectedCell = null;
            Show(AppScreen.Gameplay);
        }

        void BuildGameplay()
        {
            var p = PlayerProgress.Instance;
            AddBackground(new Color(0.05f, 0.06f, 0.12f));
            AddLabel($"Nível {_session.Level.LevelId}", 36, new Vector2(0, 880), Color.white);
            AddLabel($"Movimentos: {_session.MovesLeft}   Score: {_session.Score}", 26, new Vector2(0, 800), Color.white);
            AddLabel(ObjectiveText(), 24, new Vector2(0, 740), new Color(0.85f, 0.9f, 1f));
            AddLabel($"H:{p.Data.Hammers}  S:{p.Data.Swaps}  L:{p.Data.LineBlasts}", 22, new Vector2(0, 680), Color.white);

            if (_pendingBooster.HasValue)
                AddLabel($"Booster ativo: {_pendingBooster} — toque numa gema", 22, new Vector2(0, 630), new Color(1f, 0.85f, 0.4f));

            BuildBoard(new Vector2(0, -40), 820);
            AddButton("Martelo", new Vector2(-360, -780), () => ActivateBooster(BoosterType.Hammer), new Vector2(180, 80));
            AddButton("Troca", new Vector2(-120, -780), () => ActivateBooster(BoosterType.Swap), new Vector2(180, 80));
            AddButton("Linha", new Vector2(120, -780), () => ActivateBooster(BoosterType.LineBlast), new Vector2(180, 80));
            AddButton("Sair", new Vector2(360, -780), () =>
            {
                _lastWon = false;
                Show(AppScreen.Result);
            }, new Vector2(180, 80));
        }

        string ObjectiveText()
        {
            var parts = new System.Text.StringBuilder();
            foreach (var o in _session.Level.Objectives)
            {
                int left = _session.ObjectiveRemaining(o);
                parts.Append(o.Type switch
                {
                    ObjectiveType.CollectColor => $"Coletar {o.Color}: {left}  ",
                    ObjectiveType.Score => $"Score: {left}  ",
                    ObjectiveType.ClearBlockers => $"Bloqueios: {left}  ",
                    _ => ""
                });
            }
            return parts.ToString();
        }

        void ActivateBooster(BoosterType type)
        {
            _pendingBooster = _pendingBooster == type ? null : type;
            Rebuild();
        }

        void BuildBoard(Vector2 center, float size)
        {
            int w = _session.Board.Width;
            int h = _session.Board.Height;
            float cell = size / Mathf.Max(w, h);
            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(_root, false);
            var boardRt = boardGo.GetComponent<RectTransform>();
            boardRt.sizeDelta = new Vector2(size, size);
            boardRt.anchoredPosition = center;

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int cx = x;
                int cy = y;
                var cellData = _session.Board.Grid[x, y];
                var cellGo = new GameObject($"C{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                cellGo.transform.SetParent(boardGo.transform, false);
                var rt = cellGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cell - 6, cell - 6);
                rt.anchoredPosition = new Vector2((x - (w - 1) * 0.5f) * cell, (y - (h - 1) * 0.5f) * cell);

                var img = cellGo.GetComponent<Image>();
                img.sprite = _whiteSprite;
                img.color = GemColorToUi(cellData);
                if (_selectedCell.HasValue && _selectedCell.Value.x == x && _selectedCell.Value.y == y)
                    img.color = Color.Lerp(img.color, Color.white, 0.45f);

                int bx = cx, by = cy;
                cellGo.GetComponent<Button>().onClick.AddListener(() => OnCellClicked(bx, by));
            }
        }

        Color GemColorToUi(Cell cell)
        {
            if (cell.IsHole) return new Color(0.1f, 0.1f, 0.1f, 0.3f);
            if (cell.Blocker == BlockerType.Box) return new Color(0.45f, 0.3f, 0.15f);
            Color baseCol = cell.Color switch
            {
                GemColor.Crystal => new Color(0.85f, 0.95f, 1f),
                GemColor.Amber => new Color(1f, 0.7f, 0.2f),
                GemColor.Sapphire => new Color(0.25f, 0.45f, 1f),
                GemColor.Emerald => new Color(0.2f, 0.85f, 0.45f),
                GemColor.Ruby => new Color(0.95f, 0.25f, 0.3f),
                GemColor.Amethyst => new Color(0.7f, 0.35f, 0.95f),
                _ => Color.gray
            };
            if (cell.Blocker == BlockerType.Ice)
                baseCol = Color.Lerp(baseCol, Color.cyan, 0.45f);
            return baseCol;
        }

        void OnCellClicked(int x, int y)
        {
            if (_session.IsWon || _session.IsLost)
            {
                FinishLevel();
                return;
            }

            if (_pendingBooster == BoosterType.Hammer)
            {
                if (PlayerProgress.Instance.TryUseBooster(BoosterType.Hammer))
                    _session.TryHammer(x, y);
                _pendingBooster = null;
                AfterMove();
                return;
            }

            if (_pendingBooster == BoosterType.LineBlast)
            {
                if (PlayerProgress.Instance.TryUseBooster(BoosterType.LineBlast))
                    _session.TryLineBlast(y);
                _pendingBooster = null;
                AfterMove();
                return;
            }

            if (!_selectedCell.HasValue)
            {
                _selectedCell = (x, y);
                Rebuild();
                return;
            }

            var (sx, sy) = _selectedCell.Value;
            _selectedCell = null;
            if (sx == x && sy == y)
            {
                Rebuild();
                return;
            }

            if (_pendingBooster == BoosterType.Swap)
            {
                if (PlayerProgress.Instance.TryUseBooster(BoosterType.Swap))
                    _session.ForceSwap(sx, sy, x, y);
                _pendingBooster = null;
            }
            else
            {
                _session.TrySwap(sx, sy, x, y);
            }

            if (_session.Score > 0)
                SfxPlayer.Instance?.PlayMatch();
            else
                SfxPlayer.Instance?.PlayClick();

            AfterMove();
        }

        void AfterMove()
        {
            if (_session.IsWon || _session.IsLost)
            {
                FinishLevel();
                return;
            }
            Rebuild();
        }

        void FinishLevel()
        {
            _lastWon = _session.IsWon;
            if (_lastWon)
            {
                SfxPlayer.Instance?.PlayWin();
                PlayerProgress.Instance.OnLevelWon(_session.Level.LevelId, _session.Level.CoinReward);
            }
            else
            {
                SfxPlayer.Instance?.PlayFail();
                if (!PlayerProgress.Instance.Data.RemoveAds)
                    MonetizationHub.Instance?.Ads.ShowInterstitialIfAllowed();
            }
            Show(AppScreen.Result);
        }

        void BuildResult()
        {
            AddBackground(new Color(0.06f, 0.07f, 0.14f));
            if (_lastWon)
            {
                AddLabel("Vitória!", 56, new Vector2(0, 400), new Color(1f, 0.9f, 0.4f));
                AddLabel($"+{_session.Level.CoinReward} moedas", 32, new Vector2(0, 280), Color.white);
                AddLabel(CastleProgress.StatusText(PlayerProgress.Instance), 26, new Vector2(0, 180), new Color(0.7f, 0.85f, 1f));
                AddButton("Próximo nível", new Vector2(0, 0), () => TryStartLevel(_session.Level.LevelId + 1));
            }
            else
            {
                AddLabel("Sem movimentos", 48, new Vector2(0, 400), new Color(1f, 0.5f, 0.5f));
                AddLabel($"Continuar (+5 moves) — {PlayerProgress.ContinueCost} moedas", 24, new Vector2(0, 280), Color.white);
                AddButton("Continuar (moedas)", new Vector2(0, 120), () =>
                {
                    if (PlayerProgress.Instance.TrySpendCoins(PlayerProgress.ContinueCost))
                    {
                        _session.AddExtraMoves(5);
                        Show(AppScreen.Gameplay);
                    }
                    else
                    {
                        _statusMessage = "Moedas insuficientes — abra a loja.";
                        Show(AppScreen.Shop);
                    }
                });
                AddButton("Continuar (vídeo)", new Vector2(0, -20), () =>
                {
                    MonetizationHub.Instance.Ads.ShowRewarded(ok =>
                    {
                        if (ok)
                        {
                            _session.AddExtraMoves(5);
                            Show(AppScreen.Gameplay);
                        }
                    });
                });
            }

            AddButton("Mapa", new Vector2(0, -180), () => Show(AppScreen.LevelSelect));
            AddButton("Início", new Vector2(0, -320), () => Show(AppScreen.Home));
        }

        void BuildShop()
        {
            var iap = MonetizationHub.Instance.Iap;
            AddBackground(new Color(0.09f, 0.08f, 0.14f));
            AddLabel("Loja Lumina", 48, new Vector2(0, 820), Color.white);
            AddLabel($"Saldo: {PlayerProgress.Instance.Data.Coins} moedas", 28, new Vector2(0, 720), Color.white);
            AddButton("Voltar", new Vector2(-400, 820), () => Show(AppScreen.Home), new Vector2(180, 70));

            float y = 520;
            foreach (IapProductId id in System.Enum.GetValues(typeof(IapProductId)))
            {
                var captured = id;
                string label = $"{ProductLabel(id)} — {iap.GetPriceLabel(id)}";
                AddButton(label, new Vector2(0, y), () =>
                {
                    iap.Purchase(captured, ok =>
                    {
                        _statusMessage = ok ? $"Compra OK: {captured}" : "Falha na compra";
                        Rebuild();
                    });
                }, new Vector2(720, 90));
                y -= 110;
            }
        }

        static string ProductLabel(IapProductId id) => id switch
        {
            IapProductId.CoinsSmall => "500 moedas",
            IapProductId.CoinsMedium => "1500 moedas",
            IapProductId.CoinsLarge => "5000 moedas",
            IapProductId.LivesRefill => "Encher vidas",
            IapProductId.BoosterPack => "Pack boosters",
            IapProductId.RemoveAds => "Remover anúncios",
            _ => id.ToString()
        };

        void BuildOutOfLives()
        {
            AddBackground(new Color(0.12f, 0.05f, 0.08f));
            AddLabel("Sem vidas", 52, new Vector2(0, 300), Color.white);
            int sec = PlayerProgress.Instance.SecondsToNextLife();
            AddLabel($"Próxima vida em {sec / 60:00}:{sec % 60:00}", 28, new Vector2(0, 180), Color.white);
            AddButton("Assistir vídeo (+1 vida)", new Vector2(0, 40), () =>
            {
                MonetizationHub.Instance.Ads.ShowRewarded(ok =>
                {
                    if (ok) PlayerProgress.Instance.AddLives(1);
                    Show(AppScreen.Home);
                });
            });
            AddButton("Comprar vidas", new Vector2(0, -120), () => Show(AppScreen.Shop));
            AddButton("Voltar", new Vector2(0, -260), () => Show(AppScreen.Home));
        }

        // --- UI helpers ---

        void AddBackground(Color color)
        {
            var go = new GameObject("BG", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            Stretch(go.GetComponent<RectTransform>());
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
        }

        void AddLabel(string text, int size, Vector2 pos, Color color)
        {
            var t = CreateText(_root, text, size);
            t.color = color;
            t.rectTransform.anchoredPosition = pos;
            t.rectTransform.sizeDelta = new Vector2(1000, size + 20);
            t.alignment = TextAnchor.MiddleCenter;
        }

        void AddButton(string text, Vector2 pos, UnityEngine.Events.UnityAction action, Vector2? size = null)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size ?? new Vector2(520, 100);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = new Color(0.35f, 0.45f, 0.85f);
            go.GetComponent<Button>().onClick.AddListener(action);
            var label = CreateText(go.transform, text, 28);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
        }

        Text CreateText(Transform parent, string content, int size)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = _font != null ? _font : ResolveFont();
            t.supportRichText = false;
            t.text = content ?? "";
            t.fontSize = size;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            // Prefer legacy module (activeInputHandler=0). Safe on Editor + mobile.
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }
    }
}
