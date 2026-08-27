using System.Collections;
using LuminaMatch.Audio;
using LuminaMatch.Economy;
using LuminaMatch.Match3;
using LuminaMatch.Meta;
using LuminaMatch.Monetization;
using LuminaMatch.UI.Screens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LuminaMatch.UI
{
    public enum AppScreen
    {
        Home,
        LevelSelect,
        PreLevel,
        Gameplay,
        Result,
        Shop,
        OutOfLives,
        Palace
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
        (int x, int y)? _hintA;
        (int x, int y)? _hintB;
        float _lastInputTime;
        bool _autoHintAttempted;
        BoardPresenter _boardPresenter;
        bool _boardBusy;
        bool _lifeSpentThisAttempt;
        bool _settingsOpen;
        bool _handlingTimeOut;
        Text _timerHud;
        int _lastTimerShown = -1;
        Text _lifeRegenHud;
        int _lastLifeRegenShown = -2;

        const float AutoHintIdleSeconds = 12f;

        /// <summary>
        /// Lowers gameplay HUD/board by ~1cm on phone.
        /// Reference canvas 1080×1920; ~40–42 units ≈ 1cm at ~160dpi / similar phone density.
        /// </summary>
        const float GameplayUiDownShift = 42f;

        /// <summary>Home gear further right than the far-left edge.</summary>
        const float HomeGearX = -320f;

        /// <summary>Raises bottom boosters slightly — independent of board shift.</summary>
        const float GameplayBackUpShift = 56f;

        /// <summary>Raises menu Voltar buttons that sit too low on phone.</summary>
        const float MenuBackUpShift = 48f;

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
                // Phones: balanced. Tablets/iPad: prefer height so ±900 chrome still fits.
                scaler.matchWidthOrHeight = IsTabletLikeDisplay() ? 0.85f : 0.5f;

                var rootGo = new GameObject("Root", typeof(RectTransform));
                rootGo.transform.SetParent(canvasGo.transform, false);
                _root = rootGo.GetComponent<RectTransform>();
                Stretch(_root);
                ApplyTabletScreenFit();

                _boardPresenter = gameObject.GetComponent<BoardPresenter>();
                if (_boardPresenter == null)
                    _boardPresenter = gameObject.AddComponent<BoardPresenter>();
                _boardPresenter.CellClicked -= OnCellClicked;
                _boardPresenter.CellClicked += OnCellClicked;
                _boardPresenter.SwapRequested -= OnSwapRequested;
                _boardPresenter.SwapRequested += OnSwapRequested;

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
            Font font = null;
#if UNITY_IOS && !UNITY_EDITOR
            font = Font.CreateDynamicFontFromOSFont(new[] { "Helvetica", "Helvetica Neue", "Arial" }, 32);
#elif UNITY_ANDROID && !UNITY_EDITOR
            // BlueStacks / some Android images may lack Roboto under that exact name.
            font = Font.CreateDynamicFontFromOSFont(new[]
            {
                "Roboto", "sans-serif", "Arial", "Droid Sans", "Noto Sans"
            }, 32);
#else
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "sans-serif" }, 32);
#endif
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            if (font == null)
            {
                // Absolute last resort: empty dynamic font still lets Text components exist.
                font = Font.CreateDynamicFontFromOSFont(new[] { "sans-serif", "serif", "monospace" }, 24);
            }
            Debug.Log($"[LuminaMatch] Font resolved: {(font != null ? font.name : "NULL")}");
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
            RefreshLifeRegenHud();

            if (_screen != AppScreen.Gameplay || _session == null)
                return;

            if (!_session.IsWon && !_session.IsLost && !_settingsOpen && !_boardBusy && !_handlingTimeOut)
            {
                _session.TickTime(Time.unscaledDeltaTime);
                RefreshTimerHud();
                if (_session.TimedOut)
                {
                    FinishOrRestartIfNeeded();
                    return;
                }
            }

            if (_session.IsWon || _session.IsLost)
                return;

            if (_hintA.HasValue || _autoHintAttempted)
                return;

            if (Time.time - _lastInputTime >= AutoHintIdleSeconds)
            {
                _autoHintAttempted = true;
                ShowHint();
            }
        }

        void RefreshTimerHud()
        {
            if (_timerHud == null || _session == null) return;
            int shown = Mathf.CeilToInt(_session.TimeLeft);
            if (shown == _lastTimerShown) return;
            _lastTimerShown = shown;
            _timerHud.text = $"Tempo  {Match3Session.FormatTime(_session.TimeLeft)}";
            _timerHud.color = _session.TimeLeft <= 15f
                ? new Color(1f, 0.45f, 0.4f)
                : Color.white;
        }

        void RefreshLifeRegenHud()
        {
            if (_lifeRegenHud == null) return;
            var p = PlayerProgress.Instance;
            if (p == null) return;

            int sec = p.SecondsToNextLife();
            bool full = p.Data.Lives >= p.Data.MaxLives;
            int key = full ? -1 : sec;
            if (key == _lastLifeRegenShown) return;
            _lastLifeRegenShown = key;
            _lifeRegenHud.text = FormatLifeRegenHud(p, sec);
            _lifeRegenHud.color = full ? new Color(0.75f, 1f, 0.8f) : Color.white;
        }

        static string FormatLifeRegenHud(PlayerProgress p, int sec)
        {
            if (p.Data.Lives >= p.Data.MaxLives)
                return $"Vidas  {p.Data.Lives}/{p.Data.MaxLives}\ncheias";
            return $"Vidas  {p.Data.Lives}/{p.Data.MaxLives}\n+1 em  {FormatMmSs(sec)}";
        }

        static string FormatMmSs(int totalSeconds)
        {
            int s = Mathf.Max(0, totalSeconds);
            return $"{s / 60:00}:{s % 60:00}";
        }

        void HandleTimeOutRestart()
        {
            // Legacy name: timeout now opens the same Result continue flow as out-of-moves.
            if (_handlingTimeOut || _session == null) return;
            _handlingTimeOut = true;
            FinishLevel();
            _handlingTimeOut = false;
        }

        public void Show(AppScreen screen)
        {
            _screen = screen;
            Rebuild();
        }

        void Rebuild()
        {
            _timerHud = null;
            _lifeRegenHud = null;
            _lastTimerShown = -1;
            _lastLifeRegenShown = -2;

            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);

            switch (_screen)
            {
                case AppScreen.Home: BuildHome(); break;
                case AppScreen.LevelSelect: BuildLevelSelect(); break;
                case AppScreen.PreLevel: BuildPreLevel(); break;
                case AppScreen.Gameplay: BuildGameplay(); break;
                case AppScreen.Result: BuildResult(); break;
                case AppScreen.Shop: BuildShop(); break;
                case AppScreen.OutOfLives: BuildOutOfLives(); break;
                case AppScreen.Palace: BuildPalace(); break;
            }

            if (_settingsOpen)
                BuildSettingsModal();
        }

        void BuildHome()
        {
            var p = PlayerProgress.Instance;
            AddArtBackground(0.35f);

            var palaceGo = new GameObject("Palace", typeof(RectTransform), typeof(Image), typeof(Button));
            palaceGo.transform.SetParent(_root, false);
            var palaceRt = palaceGo.GetComponent<RectTransform>();
            palaceRt.sizeDelta = new Vector2(720, 420);
            palaceRt.anchoredPosition = new Vector2(0, 520);
            var palaceImg = palaceGo.GetComponent<Image>();
            palaceImg.sprite = PalaceStageVisual.LoadSprite(CastleProgress.UnlockedPieces(p)) ?? ArtCatalog.Background;
            palaceImg.color = Color.white;
            palaceImg.preserveAspect = true;
            palaceGo.GetComponent<Button>().onClick.AddListener(() => Show(AppScreen.Palace));
            BoardJuice.Ensure(transform).DropIn(palaceRt, 80f, 0.28f);

            AddLabel("LUMINA MATCH", 64, new Vector2(0, 280), new Color(1f, 0.9f, 0.55f));
            AddLabel("Restaure o Palácio de Luz", 32, new Vector2(0, 200), Color.white);
            AddLabel(CastleProgress.StatusText(p), 26, new Vector2(0, 140), new Color(0.7f, 0.85f, 1f));
            AddPalaceProgressBar(new Vector2(0, 95), new Vector2(520, 22), CastleProgress.Completion01(p));
            AddLabel(CastleProgress.NextPieceHint(p), 20, new Vector2(0, 55), new Color(0.85f, 0.8f, 1f));
            AddLabel($"Moedas: {p.Data.Coins}   Vidas: {p.Data.Lives}/{p.Data.MaxLives}", 26, new Vector2(0, 10), Color.white);

            string hint = TutorialDirector.HomeHint(p);
            if (!string.IsNullOrEmpty(hint))
                AddLabel(hint, 22, new Vector2(0, -100), new Color(1f, 0.9f, 0.6f));

            if (!string.IsNullOrEmpty(_statusMessage))
                AddLabel(_statusMessage, 24, new Vector2(0, -155), new Color(1f, 0.75f, 0.4f));

            AddChromeButton("Jogar", new Vector2(0, -260), () =>
            {
                _statusMessage = "";
                if (p.Data.TutorialStep < 3)
                    TryStartLevel(System.Math.Max(1, p.Data.HighestUnlockedLevel));
                else
                    Show(AppScreen.LevelSelect);
            }, new Vector2(520, 96), ButtonStyle.Primary);

            AddChromeButton("Ver palácio", new Vector2(0, -370), () => Show(AppScreen.Palace), new Vector2(420, 80), ButtonStyle.Info);

            bool showDailyOffer = OfferService.ShouldShowDailyOffer(p.Data, System.DateTime.UtcNow);
            float shopY = showDailyOffer ? -600f : -490f;
            float continueY = showDailyOffer ? -720f : -610f;

            if (showDailyOffer)
                AddChromeButton("Oferta do dia", new Vector2(0, -490), () => Show(AppScreen.Shop), new Vector2(520, 96), ButtonStyle.Active);

            AddChromeButton("Loja", new Vector2(0, shopY), () => Show(AppScreen.Shop), new Vector2(520, 96), ButtonStyle.Action);
            AddChromeButton("Mapa de níveis", new Vector2(0, continueY), () => Show(AppScreen.LevelSelect), new Vector2(520, 96), ButtonStyle.Info);
            AddChromeButton($"Continuar nível {p.Data.HighestUnlockedLevel}", new Vector2(0, continueY - 120f), () => TryStartLevel(p.Data.HighestUnlockedLevel), new Vector2(560, 96), ButtonStyle.Secondary);
            AddGearButton(new Vector2(HomeGearX, 880f));
            AddVersionLabel(new Vector2(420, -900), TextAnchor.MiddleRight);
        }

        void BuildPalace()
        {
            var p = PlayerProgress.Instance;
            int pieces = CastleProgress.UnlockedPieces(p);
            AddArtBackground(0.38f);
            AddPanel(new Vector2(0, 860), new Vector2(1000, 140), new Color(0.06f, 0.08f, 0.16f, 0.9f));
            AddLabel("Palácio de Luz", 44, new Vector2(0, 910), new Color(1f, 0.92f, 0.65f));
            AddVersionLabel(new Vector2(420, 910), TextAnchor.MiddleRight);
            AddChromeButton("Início", new Vector2(-420, 900), () => Show(AppScreen.Home), new Vector2(160, 72), ButtonStyle.Secondary);

            var heroGo = new GameObject("PalaceHero", typeof(RectTransform), typeof(Image));
            heroGo.transform.SetParent(_root, false);
            var heroRt = heroGo.GetComponent<RectTransform>();
            heroRt.sizeDelta = new Vector2(640, 340);
            heroRt.anchoredPosition = new Vector2(0, 560);
            var heroImg = heroGo.GetComponent<Image>();
            heroImg.sprite = PalaceStageVisual.LoadSprite(pieces) ?? ArtCatalog.Background;
            heroImg.preserveAspect = true;
            heroImg.color = Color.white;
            heroImg.raycastTarget = false;
            var juice = BoardJuice.Ensure(transform);
            juice.PalaceReveal(heroRt);
            juice.SparkleBurst(_root, _whiteSprite, new Vector2(0, 560), 10);

            AddLabel(CastleProgress.StatusText(p), 28, new Vector2(0, 340), new Color(0.75f, 0.88f, 1f));
            AddPalaceProgressBar(new Vector2(0, 290), new Vector2(560, 24), CastleProgress.Completion01(p));
            AddLabel(PalaceStageVisual.StageCaption(p), 22, new Vector2(0, 245), new Color(0.9f, 0.85f, 1f));
            AddLabel(CastleProgress.NextPieceHint(p), 20, new Vector2(0, 205), new Color(1f, 0.88f, 0.55f));

            var content = CreateVerticalScroll(0.05f, 0.52f);
            for (int i = 0; i < CastleProgress.TotalPieces; i++)
            {
                bool unlocked = i < pieces;
                string name = CastleProgress.PieceName(i);
                string text = unlocked
                    ? $"✓  {i + 1}. {name}"
                    : $"○  {i + 1}. {name}";
                AddPalacePieceRow(content, text, unlocked);
            }

            AddChromeButton("Jogar", new Vector2(0, -880), () =>
            {
                if (p.Data.TutorialStep < 3)
                    TryStartLevel(System.Math.Max(1, p.Data.HighestUnlockedLevel));
                else
                    Show(AppScreen.LevelSelect);
            }, new Vector2(420, 84), ButtonStyle.Primary);
        }

        void AddPalacePieceRow(Transform parent, string text, bool unlocked)
        {
            var go = new GameObject("Piece", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 72;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = unlocked
                ? new Color(0.2f, 0.38f, 0.28f, 0.95f)
                : new Color(0.14f, 0.14f, 0.18f, 0.9f);
            img.raycastTarget = false;
            var label = CreateText(go.transform, text, 24);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleLeft;
            label.color = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.6f);
            label.rectTransform.offsetMin = new Vector2(28f, 0f);
            label.rectTransform.offsetMax = new Vector2(-16f, 0f);
        }

        void BuildLevelSelect()
        {
            var p = PlayerProgress.Instance;
            AddArtBackground(0.4f);
            AddPanel(new Vector2(0, 860), new Vector2(1000, 120), new Color(0.06f, 0.08f, 0.16f, 0.88f));
            AddLabel("Mapa", 44, new Vector2(0, 890), new Color(1f, 0.92f, 0.65f));
            AddVersionLabel(new Vector2(420, 890), TextAnchor.MiddleRight);
            AddLabel($"Vidas {p.Data.Lives}/{p.Data.MaxLives}   ·   Moedas {p.Data.Coins}", 24, new Vector2(0, 820), new Color(0.85f, 0.9f, 1f));
            AddChromeButton("Início", new Vector2(-420, 880), () => Show(AppScreen.Home), new Vector2(160, 72), ButtonStyle.Secondary);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollGo.transform.SetParent(_root, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0.08f);
            scrollRt.anchorMax = new Vector2(0.95f, 0.78f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.55f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = true;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scrollGo.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.padding = new RectOffset(24, 24, 24, 24);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            int current = p.Data.HighestUnlockedLevel;
            RectTransform focusRow = null;
            for (int i = 1; i <= LevelCatalog.TotalLevels; i++)
            {
                int level = i;
                bool unlocked = LevelMap.IsUnlocked(p, level);
                bool isCurrent = unlocked && level == current;
                bool cleared = unlocked && level < current;
                var row = new GameObject($"L{level}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                row.transform.SetParent(content.transform, false);
                row.GetComponent<LayoutElement>().preferredHeight = 108;
                row.GetComponent<Image>().color = !unlocked
                    ? new Color(0.14f, 0.14f, 0.18f, 0.95f)
                    : isCurrent
                        ? new Color(0.45f, 0.38f, 0.18f, 0.98f)
                        : cleared
                            ? new Color(0.16f, 0.36f, 0.28f, 0.95f)
                            : new Color(0.18f, 0.28f, 0.48f, 0.95f);
                var btn = row.GetComponent<Button>();
                btn.interactable = unlocked;
                int captured = level;
                btn.onClick.AddListener(() => TryStartLevel(captured));

                string title = LevelCatalog.Get(level).Title;
                string text = !unlocked
                    ? $"Nível {level}  ·  bloqueado"
                    : isCurrent
                        ? $"Nível {level}  ·  {title}\n▶ Continuar"
                        : cleared
                            ? $"Nível {level}  ·  {title}\n✓ Concluído"
                            : $"Nível {level}  ·  {title}";
                var label = CreateText(row.transform, text, unlocked ? 26 : 24);
                Stretch(label.rectTransform);
                label.alignment = TextAnchor.MiddleCenter;
                label.color = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.6f);
                if (isCurrent) focusRow = row.GetComponent<RectTransform>();
            }

            if (focusRow != null)
            {
                Canvas.ForceUpdateCanvases();
                // Scroll so current level sits near the top third of the viewport.
                float contentH = Mathf.Max(1f, contentRt.rect.height);
                float viewH = scrollRt.rect.height;
                float rowY = -focusRow.anchoredPosition.y;
                float norm = 1f - Mathf.Clamp01((rowY - viewH * 0.25f) / Mathf.Max(1f, contentH - viewH));
                scroll.verticalNormalizedPosition = norm;
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

            if (levelId > LevelCatalog.TotalLevels)
            {
                _statusMessage = "Você concluiu todos os níveis!";
                Show(AppScreen.Home);
                return;
            }

            _selectedLevel = levelId;
            Show(AppScreen.PreLevel);
        }

        void ConfirmStartLevel()
        {
            var p = PlayerProgress.Instance;
            // Lives are spent only on loss (or abandon), not on start.
            if (!p.HasLife())
            {
                Show(AppScreen.OutOfLives);
                return;
            }

            _session = new Match3Session(LevelCatalog.Get(_selectedLevel));
            _pendingBooster = null;
            _selectedCell = null;
            _hintA = _hintB = null;
            _lifeSpentThisAttempt = false;
            ResetIdleHintTimer();
            Show(AppScreen.Gameplay);
        }

        void SpendLifeOnFailureOnce()
        {
            if (_lifeSpentThisAttempt) return;
            if (PlayerProgress.Instance.TrySpendLife())
                _lifeSpentThisAttempt = true;
        }

        void AbandonLevelToMap()
        {
            if (_session != null && !_session.IsWon)
                SpendLifeOnFailureOnce();
            _session = null;
            Show(AppScreen.LevelSelect);
        }

        void BuildPreLevel()
        {
            var p = PlayerProgress.Instance;
            var level = LevelCatalog.Get(_selectedLevel);
            AddArtBackground(0.45f);
            AddPanel(new Vector2(0, 120), new Vector2(920, 1100), new Color(0.05f, 0.07f, 0.14f, 0.82f));
            AddLabel(PreLevelCopy.Title(level), 42, new Vector2(0, 520), new Color(1f, 0.92f, 0.6f));
            AddLabel(PreLevelCopy.Objectives(level), 26, new Vector2(0, 220), Color.white);
            AddLabel(PreLevelCopy.BoosterSummary(p), 22, new Vector2(0, -20), new Color(0.85f, 0.9f, 1f));
            AddLabel($"Vidas {p.Data.Lives}/{p.Data.MaxLives}   ·   Moedas {p.Data.Coins}", 24, new Vector2(0, -110), new Color(0.9f, 0.85f, 0.55f));
            AddLabel("Vidas só caem se você perder (ou sair do nível)", 20, new Vector2(0, -180), new Color(0.75f, 0.85f, 1f));
            AddChromeButton("Jogar", new Vector2(0, -280), ConfirmStartLevel, new Vector2(560, 110), ButtonStyle.Primary);
            AddChromeButton("Mapa", new Vector2(0, -420), () => Show(AppScreen.LevelSelect), new Vector2(360, 80), ButtonStyle.Secondary);
        }

        void BuildGameplay()
        {
            var p = PlayerProgress.Instance;
            AddArtBackground(0.42f);

            // Fixed vertical anchors from pre-maximize era (0.1.16–0.1.19).
            // Keep 0.1.21 board SIZE math; do not pack chrome upward into free space.
            float dy = -GameplayUiDownShift;
            float boostY = -760f + GameplayBackUpShift;
            float backY = -980f;

            Canvas.ForceUpdateCanvases();
            GetGameplayCanvasSize(out float canvasW, out _);
            float halfW = canvasW * 0.5f;

            const float boosterPanelH = 96f;
            const float mapaH = 70f;
            const float objectiveH = 84f;
            const float sideMargin = 6f;
            const float gapFrameHud = 8f;
            const float gapFrameBoost = 10f;

            float hudTopY = 900f + dy;
            float movesY = 820f + dy;
            float objectiveY = 720f + dy;
            float hintY = 650f + dy;
            float boardY = -30f + dy;

            AddPanel(new Vector2(0, 860f + dy), new Vector2(Mathf.Max(720f, canvasW) - 80f, 160f), new Color(0.04f, 0.06f, 0.12f, 0.86f));
            AddHudChip($"Nível {_session.Level.LevelId}", new Vector2(0, hudTopY), new Vector2(220, 56), new Color(0.28f, 0.32f, 0.55f, 0.95f));
            AddHudChip($"Moedas  {p.Data.Coins}", new Vector2(halfW - 180f, hudTopY), new Vector2(260, 56), new Color(0.45f, 0.35f, 0.12f, 0.95f));
            AddLifeRegenDisplay(new Vector2(-halfW + 180f, hudTopY), new Vector2(280, 64));

            AddHudChip($"Movimentos  {_session.MovesLeft}", new Vector2(-280, movesY), new Vector2(240, 52), new Color(0.15f, 0.28f, 0.42f, 0.95f));
            _timerHud = AddHudChipText(
                $"Tempo  {Match3Session.FormatTime(_session.TimeLeft)}",
                new Vector2(0, movesY),
                new Vector2(220, 52),
                _session.TimeLeft <= 15f ? new Color(0.55f, 0.18f, 0.2f, 0.95f) : new Color(0.15f, 0.28f, 0.42f, 0.95f));
            _lastTimerShown = Mathf.CeilToInt(_session.TimeLeft);
            AddHudChip($"Pontos  {_session.Score}", new Vector2(280, movesY), new Vector2(240, 52), new Color(0.15f, 0.28f, 0.42f, 0.95f));

            BuildObjectiveBar(new Vector2(0, objectiveY), objectiveH);

            string tut = TutorialDirector.GameplayHint(_session.Level.LevelId, PlayerProgress.Instance.Data.TutorialStep);
            if (!string.IsNullOrEmpty(tut))
                AddLabel(tut, 18, new Vector2(0, hintY), new Color(1f, 0.9f, 0.55f));
            else if (_pendingBooster.HasValue)
                AddLabel($"Toque numa gema para usar {UiLabels.Booster(_pendingBooster.Value)}", 18, new Vector2(0, hintY), new Color(1f, 0.85f, 0.4f));
            else
                AddLabel("Arraste uma gema para trocar", 18, new Vector2(0, hintY), new Color(0.75f, 0.85f, 1f));

            // Board size first (reserve space under hint for centered gear), then place gear mid-gap.
            const float gearSlot = 96f;
            float availTop = hintY - 18f - gearSlot - gapFrameHud;
            float boostTop = boostY + boosterPanelH * 0.5f;
            float availBot = boostTop + gapFrameBoost;
            float maxVisibleH = Mathf.Max(200f, availTop - availBot);
            float maxVisibleW = Mathf.Max(200f, canvasW - 2f * sideMargin);
            float boardSize = BoardPresenter.MaxBoardSizeForVisible(maxVisibleW, maxVisibleH);

            Vector2 contentSize = BoardPresenter.ContentSizeForBoard(boardSize);
            float frameTop = boardY + contentSize.y * 0.5f;
            float gearY = (hintY + frameTop) * 0.5f;
            AddGearButton(new Vector2(0, gearY));

            _boardPresenter.Bind(_root, _session.Board, new Vector2(0, boardY), boardSize, _whiteSprite);
            _boardPresenter.SetInputLocked(_boardBusy || _settingsOpen);
            _boardPresenter.Refresh(_selectedCell);
            if (_hintA.HasValue)
            {
                var juice = BoardJuice.Ensure(transform);
                var a = _boardPresenter.CellRect(_hintA.Value.x, _hintA.Value.y);
                var b = _hintB.HasValue ? _boardPresenter.CellRect(_hintB.Value.x, _hintB.Value.y) : null;
                if (a != null) juice.Punch(a);
                if (b != null) juice.Punch(b);
                SfxPlayer.Instance?.PlayHint();
            }

            AddPanel(new Vector2(0, boostY), new Vector2(canvasW - 36f, boosterPanelH), new Color(0.04f, 0.06f, 0.12f, 0.9f));
            float btnW = Mathf.Min(200f, (canvasW - 72f) / 4.2f);
            AddChromeButton("Dica", new Vector2(-1.5f * (btnW + 14f), boostY), ShowHint, new Vector2(btnW, 84), ButtonStyle.Info);
            AddChromeButton($"Martelo\n×{p.Data.Hammers}", new Vector2(-0.5f * (btnW + 14f), boostY), () => ActivateBooster(BoosterType.Hammer), new Vector2(btnW, 84),
                _pendingBooster == BoosterType.Hammer ? ButtonStyle.Active : ButtonStyle.Action);
            AddChromeButton($"Troca\n×{p.Data.Swaps}", new Vector2(0.5f * (btnW + 14f), boostY), () => ActivateBooster(BoosterType.Swap), new Vector2(btnW, 84),
                _pendingBooster == BoosterType.Swap ? ButtonStyle.Active : ButtonStyle.Action);
            AddChromeButton($"Linha\n×{p.Data.LineBlasts}", new Vector2(1.5f * (btnW + 14f), boostY), () => ActivateBooster(BoosterType.LineBlast), new Vector2(btnW, 84),
                _pendingBooster == BoosterType.LineBlast ? ButtonStyle.Active : ButtonStyle.Action);
            AddChromeButton("Mapa", new Vector2(0, backY), AbandonLevelToMap, new Vector2(230, mapaH), ButtonStyle.Secondary);
            AddVersionLabel(new Vector2(halfW - 64f, backY), TextAnchor.MiddleRight);
        }

        /// <summary>One row: Objetivo · [gem] Esmeralda ×20 (no duplicate second line).</summary>
        void BuildObjectiveBar(Vector2 center, float height = 84f)
        {
            AddPanel(center, new Vector2(Mathf.Max(720f, _root.rect.width) - 80f, height), new Color(0.08f, 0.1f, 0.18f, 0.85f));
            AddLabel("Objetivo", 20, new Vector2(center.x - 300f, center.y), new Color(0.75f, 0.82f, 1f));

            var objectives = _session.Level.Objectives;
            if (objectives == null || objectives.Length == 0) return;

            float x = center.x - 70f;
            foreach (var o in objectives)
            {
                int left = _session.ObjectiveRemaining(o);
                if (o.Type == ObjectiveType.CollectColor)
                {
                    var gemGo = new GameObject("ObjGem", typeof(RectTransform), typeof(Image));
                    gemGo.transform.SetParent(_root, false);
                    var gemRt = gemGo.GetComponent<RectTransform>();
                    gemRt.sizeDelta = new Vector2(56, 56);
                    gemRt.anchoredPosition = new Vector2(x, center.y);
                    var gemImg = gemGo.GetComponent<Image>();
                    gemImg.sprite = ArtCatalog.Gem(o.Color);
                    gemImg.preserveAspect = true;
                    gemImg.color = Color.white;
                    gemImg.raycastTarget = false;

                    AddLabel($"{UiLabels.Gem(o.Color)}  ×{left}", 26, new Vector2(x + 120f, center.y), Color.white);
                    x += 300f;
                }
                else if (o.Type == ObjectiveType.Score)
                {
                    AddLabel($"Pontos  ×{left}", 24, new Vector2(x + 70f, center.y), Color.white);
                    x += 240f;
                }
                else if (o.Type == ObjectiveType.ClearBlockers)
                {
                    var boxGo = new GameObject("ObjBox", typeof(RectTransform), typeof(Image));
                    boxGo.transform.SetParent(_root, false);
                    var boxRt = boxGo.GetComponent<RectTransform>();
                    boxRt.sizeDelta = new Vector2(56, 56);
                    boxRt.anchoredPosition = new Vector2(x, center.y);
                    var boxImg = boxGo.GetComponent<Image>();
                    boxImg.sprite = ArtCatalog.Box;
                    boxImg.preserveAspect = true;
                    boxImg.color = Color.white;
                    boxImg.raycastTarget = false;
                    AddLabel($"Obstáculos  ×{left}", 24, new Vector2(x + 120f, center.y), Color.white);
                    x += 300f;
                }
            }
        }

        void ResetIdleHintTimer()
        {
            _lastInputTime = Time.time;
            _autoHintAttempted = false;
        }

        void ShowHint()
        {
            if (HintFinder.TryFindHint(_session.Board.Grid, out int x1, out int y1, out int x2, out int y2))
            {
                _hintA = (x1, y1);
                _hintB = (x2, y2);
                _selectedCell = (x1, y1);
            }
            else
            {
                _statusMessage = "Sem jogadas óbvias — use um poder.";
                _hintA = _hintB = null;
            }
            Rebuild();
        }

        string ObjectiveText()
        {
            var parts = new System.Text.StringBuilder("Objetivo  ·  ");
            bool first = true;
            foreach (var o in _session.Level.Objectives)
            {
                int left = _session.ObjectiveRemaining(o);
                if (!first) parts.Append("   ");
                first = false;
                parts.Append(o.Type switch
                {
                    ObjectiveType.CollectColor => $"{UiLabels.Gem(o.Color)} ×{left}",
                    ObjectiveType.Score => $"Pontos ×{left}",
                    ObjectiveType.ClearBlockers => $"Obstáculos ×{left}",
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

        void OnSwapRequested(int x1, int y1, int x2, int y2)
        {
            if (_boardBusy || _settingsOpen) return;
            if (_pendingBooster == BoosterType.Hammer || _pendingBooster == BoosterType.LineBlast)
                return; // boosters use tap, not drag

            ResetIdleHintTimer();
            _hintA = _hintB = null;
            _selectedCell = null;

            if (_session.IsWon || _session.IsLost)
            {
                FinishOrRestartIfNeeded();
                return;
            }

            StartCoroutine(PlaySwapThenResolve(x1, y1, x2, y2));
        }

        void OnCellClicked(int x, int y)
        {
            if (_boardBusy || _settingsOpen) return;

            ResetIdleHintTimer();
            _hintA = _hintB = null;

            if (_session.IsWon || _session.IsLost)
            {
                FinishOrRestartIfNeeded();
                return;
            }

            if (_pendingBooster == BoosterType.Hammer)
            {
                var beforeHammer = CaptureBoardColors();
                var beforePowersH = CaptureBoardPowers();
                var beforeBlockersH = CaptureBoardBlockers();
                bool used = false;
                if (PlayerProgress.Instance.TryUseBooster(BoosterType.Hammer))
                    used = _session.TryHammer(x, y);
                _pendingBooster = null;
                if (used) Haptics.PulseMove();
                AfterMove(beforeHammer, beforePowersH, beforeBlockersH);
                return;
            }

            if (_pendingBooster == BoosterType.LineBlast)
            {
                var beforeLine = CaptureBoardColors();
                var beforePowersL = CaptureBoardPowers();
                var beforeBlockersL = CaptureBoardBlockers();
                bool used = false;
                if (PlayerProgress.Instance.TryUseBooster(BoosterType.LineBlast))
                    used = _session.TryLineBlast(y);
                _pendingBooster = null;
                if (used) Haptics.PulseMove();
                AfterMove(beforeLine, beforePowersL, beforeBlockersL);
                return;
            }

            // Tap-tap fallback (also used by Swap booster).
            if (!_selectedCell.HasValue)
            {
                _selectedCell = (x, y);
                Rebuild();
                var juice = BoardJuice.Ensure(transform);
                var selected = _boardPresenter != null ? _boardPresenter.CellRect(x, y) : null;
                if (selected != null)
                    juice.Punch(selected);
                return;
            }

            var (sx, sy) = _selectedCell.Value;
            _selectedCell = null;
            if (sx == x && sy == y)
            {
                Rebuild();
                return;
            }

            StartCoroutine(PlaySwapThenResolve(sx, sy, x, y));
        }

        IEnumerator PlaySwapThenResolve(int sx, int sy, int x, int y)
        {
            _boardBusy = true;
            _boardPresenter?.SetInputLocked(true);
            var juice = BoardJuice.Ensure(transform);
            var a = _boardPresenter != null ? _boardPresenter.CellRoot(sx, sy) : null;
            var b = _boardPresenter != null ? _boardPresenter.CellRoot(x, y) : null;

            bool slideDone = false;
            if (a != null && b != null)
                juice.SwapSlide(a, b, 0.15f, () => slideDone = true);
            else
                slideDone = true;
            while (!slideDone) yield return null;

            var before = CaptureBoardColors();
            var beforePowers = CaptureBoardPowers();
            var beforeBlockers = CaptureBoardBlockers();
            bool moved;
            if (_pendingBooster == BoosterType.Swap)
            {
                moved = PlayerProgress.Instance.TryUseBooster(BoosterType.Swap)
                    && _session.ForceSwap(sx, sy, x, y);
                _pendingBooster = null;
            }
            else
                moved = _session.TrySwap(sx, sy, x, y);

            if (moved)
            {
                SfxPlayer.Instance?.PlayMatch();
                Haptics.PulseMove();
                if (_session.Score >= 120)
                    SfxPlayer.Instance?.PlayPower();
            }
            else
                SfxPlayer.Instance?.PlayClick();

            if (!moved)
            {
                _boardBusy = false;
                _boardPresenter?.SetInputLocked(false);
            }
            AfterMove(moved ? before : null, moved ? beforePowers : null, moved ? beforeBlockers : null);
        }

        void AfterMove(GemColor[,] beforeColors, BoardPowerType[,] beforePowers = null, BlockerType[,] beforeBlockers = null)
        {
            if (_session.IsWon || _session.IsLost)
            {
                _boardBusy = false;
                _boardPresenter?.SetInputLocked(false);
                FinishOrRestartIfNeeded();
                return;
            }

            if (beforeColors == null)
            {
                _boardBusy = false;
                _boardPresenter?.SetInputLocked(false);
                Rebuild();
                return;
            }

            StartCoroutine(AnimateBoardResolve(beforeColors, beforePowers, beforeBlockers));
        }

        GemColor[,] CaptureBoardColors()
        {
            var g = _session.Board.Grid;
            int w = _session.Board.Width, h = _session.Board.Height;
            var snap = new GemColor[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                snap[x, y] = g[x, y].Color;
            return snap;
        }

        BoardPowerType[,] CaptureBoardPowers()
        {
            var g = _session.Board.Grid;
            int w = _session.Board.Width, h = _session.Board.Height;
            var snap = new BoardPowerType[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                snap[x, y] = g[x, y].Power;
            return snap;
        }

        BlockerType[,] CaptureBoardBlockers()
        {
            var g = _session.Board.Grid;
            int w = _session.Board.Width, h = _session.Board.Height;
            var snap = new BlockerType[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                snap[x, y] = g[x, y].Blocker;
            return snap;
        }

        IEnumerator AnimateBoardResolve(GemColor[,] before, BoardPowerType[,] beforePowers, BlockerType[,] beforeBlockers)
        {
            _boardBusy = true;
            var juice = BoardJuice.Ensure(transform);
            var board = _session.Board;
            int w = board.Width, h = board.Height;

            // Power FX (rocket / bomb / color disk) before generic pops.
            bool powerFxPlayed = false;
            if (_boardPresenter != null && _boardPresenter.GridRoot != null)
            {
                var rocketFx = _session.ConsumeRocketFx();
                var bombFxPeek = _session.ConsumeBombFx();
                var diskFxPeek = _session.ConsumeColorDiskFx();
                // Re-queue consumed lists by keeping local copies (already consumed).
                bool isCombo = (rocketFx.Count > 0 ? 1 : 0)
                    + (bombFxPeek.Count > 0 ? 1 : 0)
                    + (diskFxPeek.Count > 0 ? 1 : 0) >= 2
                    || rocketFx.Count >= 2 || bombFxPeek.Count >= 2 || diskFxPeek.Count >= 2;
                if (isCombo)
                {
                    SfxPlayer.Instance?.PlayCombo();
                    Haptics.PulseMove();
                    bool pulseDone = false;
                    juice.ComboPulse(_boardPresenter.GridRoot, _whiteSprite, () => pulseDone = true);
                    while (!pulseDone) yield return null;
                }

                if (rocketFx.Count > 0)
                {
                    var fx = rocketFx[0];
                    bool horizontal = fx.Horizontal;
                    var cells = new System.Collections.Generic.List<RectTransform>();
                    if (horizontal)
                    {
                        for (int i = 0; i < w; i++)
                        {
                            if (before[i, fx.Line] == GemColor.None) continue;
                            var rt = _boardPresenter.CellRoot(i, fx.Line);
                            if (rt != null) cells.Add(rt);
                        }
                    }
                    else
                    {
                        for (int j = h - 1; j >= 0; j--)
                        {
                            if (before[fx.Line, j] == GemColor.None) continue;
                            var rt = _boardPresenter.CellRoot(fx.Line, j);
                            if (rt != null) cells.Add(rt);
                        }
                    }

                    if (cells.Count > 0)
                    {
                        powerFxPlayed = true;
                        SfxPlayer.Instance?.PlayRocket();
                        bool rocketDone = false;
                        juice.RocketBlast(
                            _boardPresenter.GridRoot,
                            ArtCatalog.Rocket,
                            _whiteSprite,
                            cells,
                            horizontal,
                            () => rocketDone = true);
                        while (!rocketDone) yield return null;
                    }
                }

                if (bombFxPeek.Count > 0)
                {
                    var fx = bombFxPeek[0];
                    var origin = _boardPresenter.CellRoot(fx.OriginX, fx.OriginY);
                    var cells = new System.Collections.Generic.List<RectTransform>();
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = fx.OriginX + dx, ny = fx.OriginY + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (before[nx, ny] == GemColor.None && (beforeBlockers == null || beforeBlockers[nx, ny] == BlockerType.None))
                            continue;
                        var rt = _boardPresenter.CellRoot(nx, ny);
                        if (rt != null) cells.Add(rt);
                    }
                    if (origin != null)
                    {
                        powerFxPlayed = true;
                        SfxPlayer.Instance?.PlayBomb();
                        bool bombDone = false;
                        juice.BombBlast(
                            _boardPresenter.GridRoot,
                            ArtCatalog.Bomb,
                            _whiteSprite,
                            origin,
                            cells,
                            () => bombDone = true);
                        while (!bombDone) yield return null;
                    }
                }

                if (diskFxPeek.Count > 0)
                {
                    var fx = diskFxPeek[0];
                    var origin = _boardPresenter.CellRoot(fx.OriginX, fx.OriginY);
                    var cells = new System.Collections.Generic.List<RectTransform>();
                    for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        if (before[x, y] != fx.Color) continue;
                        var rt = _boardPresenter.CellRoot(x, y);
                        if (rt != null) cells.Add(rt);
                    }
                    if (origin != null)
                    {
                        powerFxPlayed = true;
                        SfxPlayer.Instance?.PlayColorDisk();
                        bool diskDone = false;
                        juice.ColorDiskBlast(
                            _boardPresenter.GridRoot,
                            ArtCatalog.ColorDisk,
                            _whiteSprite,
                            UiLabels.GemTint(fx.Color),
                            origin,
                            cells,
                            () => diskDone = true);
                        while (!diskDone) yield return null;
                    }
                }
            }

            // Break blockers that disappeared (ice/box) with a quick pop.
            if (beforeBlockers != null && _boardPresenter != null)
            {
                int blockerPops = 0;
                bool boxSfx = false;
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (beforeBlockers[x, y] == BlockerType.None) continue;
                    if (board.Grid[x, y].Blocker != BlockerType.None) continue;
                    var rt = _boardPresenter.CellRoot(x, y);
                    if (rt == null) continue;
                    if (beforeBlockers[x, y] == BlockerType.Box) boxSfx = true;
                    blockerPops++;
                    juice.PopOut(rt, () => blockerPops--);
                }
                if (boxSfx) SfxPlayer.Instance?.PlayBoxBreak();
                float bw = 0f;
                while (blockerPops > 0 && bw < 0.35f)
                {
                    bw += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Pop gems that changed while UI still shows the previous board.
            int pops = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (before[x, y] == board.Grid[x, y].Color) continue;
                if (before[x, y] == GemColor.None) continue;
                var rt = _boardPresenter != null ? _boardPresenter.CellRect(x, y) : null;
                if (rt == null) continue;
                // Already cascaded by power FX (CellRoot was scaled)
                var root = _boardPresenter.CellRoot(x, y);
                if (powerFxPlayed && root != null && root.localScale.x < 0.05f) continue;
                pops++;
                juice.PopOut(rt, () => pops--);
            }

            float popWait = 0f;
            while (pops > 0 && popWait < 0.45f)
            {
                popWait += Time.unscaledDeltaTime;
                yield return null;
            }

            Rebuild();

            // Drop-in gems that are new or replaced (fall + refill).
            float cell = _boardPresenter != null ? _boardPresenter.CellSize : 820f / Mathf.Max(w, h);
            int drops = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var color = board.Grid[x, y].Color;
                if (color == GemColor.None || color == before[x, y]) continue;
                var rt = _boardPresenter != null ? _boardPresenter.CellRect(x, y) : null;
                if (rt == null) continue;
                drops++;
                float from = cell * (1.6f + (h - 1 - y) * 0.55f);
                juice.DropIn(rt, from, 0.22f + (h - 1 - y) * 0.015f, () => drops--);
            }

            float dropWait = 0f;
            while (drops > 0 && dropWait < 0.9f)
            {
                dropWait += Time.unscaledDeltaTime;
                yield return null;
            }

            _boardBusy = false;
            _boardPresenter?.SetInputLocked(false);
            if (_session != null && (_session.IsWon || _session.IsLost))
                FinishOrRestartIfNeeded();
        }

        void FinishOrRestartIfNeeded()
        {
            if (_session == null) return;
            if (_session.IsWon || _session.IsLost)
                FinishLevel();
        }

        void ResumeAfterDefeat()
        {
            if (_session == null) return;
            _statusMessage = "";
            _session.AddExtraMoves(5);
            if (_session.TimedOut)
                _session.RecoverFromTimeout(60f);
            _lifeSpentThisAttempt = false;
            ResetIdleHintTimer();
            Show(AppScreen.Gameplay);
        }

        void FinishLevel()
        {
            if (_session == null) return;
            _boardBusy = false;
            _boardPresenter?.SetInputLocked(false);
            _settingsOpen = false;
            _lastWon = _session.IsWon;
            if (_lastWon)
            {
                SfxPlayer.Instance?.PlayWin();
                PlayerProgress.Instance.OnLevelWon(_session.Level.LevelId, _session.Level.CoinReward);
                TutorialDirector.OnLevelCompleted(PlayerProgress.Instance, _session.Level.LevelId, true);
            }
            else
            {
                SpendLifeOnFailureOnce();
                SfxPlayer.Instance?.PlayFail();
                if (!PlayerProgress.Instance.Data.RemoveAds)
                    MonetizationHub.Instance?.Ads.ShowInterstitialIfAllowed();
            }
            Show(AppScreen.Result);
        }

        void BuildResult()
        {
            AddBackground(new Color(0.06f, 0.07f, 0.14f));
            // Nav buttons always stack BELOW content (palace / offers / continue).
            float navY = -520f;

            if (_lastWon)
            {
                var progress = PlayerProgress.Instance;
                int pieces = CastleProgress.UnlockedPieces(progress);
                bool newPiece = progress.Data.LevelsWon > 0
                    && progress.Data.LevelsWon % PlayerProgress.LevelsPerCastlePiece == 0
                    && pieces > 0;

                AddLabel("Vitória!", 56, new Vector2(0, 560), new Color(1f, 0.9f, 0.4f));
                AddLabel($"+{_session.Level.CoinReward} moedas", 32, new Vector2(0, 470), Color.white);
                AddLabel(CastleProgress.StatusText(progress), 26, new Vector2(0, 400), new Color(0.7f, 0.85f, 1f));
                AddPalaceProgressBar(new Vector2(0, 355), new Vector2(480, 20), CastleProgress.Completion01(progress));
                if (newPiece)
                    AddLabel($"Nova peça: {CastleProgress.PieceName(pieces - 1)}", 28, new Vector2(0, 310), new Color(1f, 0.85f, 0.45f));
                else
                    AddLabel(CastleProgress.NextPieceHint(progress), 22, new Vector2(0, 310), new Color(0.9f, 0.85f, 1f));

                var revealGo = new GameObject("PalaceReveal", typeof(RectTransform), typeof(Image));
                revealGo.transform.SetParent(_root, false);
                var revealRt = revealGo.GetComponent<RectTransform>();
                revealRt.sizeDelta = new Vector2(480, 280);
                revealRt.anchoredPosition = new Vector2(0, 80);
                var revealImg = revealGo.GetComponent<Image>();
                revealImg.sprite = PalaceStageVisual.LoadSprite(pieces) ?? ArtCatalog.Background;
                revealImg.preserveAspect = true;
                revealImg.color = Color.white;
                revealImg.raycastTarget = false;
                var juice = BoardJuice.Ensure(transform);
                juice.PalaceReveal(revealRt);
                juice.SparkleBurst(_root, _whiteSprite, new Vector2(0, 80), newPiece ? 14 : 8);
                if (newPiece)
                    SfxPlayer.Instance?.PlayPalace();

                AddChromeButton("Ver palácio", new Vector2(0, -100), () => Show(AppScreen.Palace), new Vector2(360, 72), ButtonStyle.Info);

                var data = PlayerProgress.Instance.Data;
                if (OfferService.ShouldShowStarterPack(data) && !data.StarterPackSeen)
                {
                    data.StarterPackSeen = true;
                    PlayerProgress.Instance.Save();
                    AddLabel("Oferta de estreia!", 28, new Vector2(0, -200), new Color(1f, 0.85f, 0.4f));
                    AddButton($"Pacote estreia — {MonetizationHub.Instance.Iap.GetPriceLabel(IapProductId.StarterPack)}", new Vector2(0, -320), () =>
                    {
                        MonetizationHub.Instance.Iap.Purchase(IapProductId.StarterPack, _ => Show(AppScreen.Home));
                    });
                    AddButton("Agora não", new Vector2(0, -440), () => TryStartLevel(_session.Level.LevelId + 1));
                    navY = -600f;
                }
                else
                {
                    AddButton("Próximo nível", new Vector2(0, -220), () => TryStartLevel(_session.Level.LevelId + 1));
                    navY = -380f;
                }
            }
            else
            {
                int levelId = _session.Level.LevelId;
                bool timedOut = _session.TimedOut;
                bool videoReady = MonetizationHub.Instance?.Ads.IsRewardedReady ?? false;
                AddLabel(timedOut ? "Tempo esgotado" : "Sem movimentos", 48, new Vector2(0, 420), new Color(1f, 0.5f, 0.5f));
                string continueHint = timedOut
                    ? $"Continuar (+5 movimentos e +1:00) — {PlayerProgress.ContinueCost} moedas"
                    : $"Continuar (+5 movimentos) — {PlayerProgress.ContinueCost} moedas";
                AddLabel(continueHint, 22, new Vector2(0, 320), Color.white);
                if (!string.IsNullOrEmpty(_statusMessage))
                    AddLabel(_statusMessage, 22, new Vector2(0, 260), new Color(1f, 0.75f, 0.4f));

                AddButton("Continuar (moedas)", new Vector2(0, 160), () =>
                {
                    if (PlayerProgress.Instance.TrySpendCoins(PlayerProgress.ContinueCost))
                        ResumeAfterDefeat();
                    else
                    {
                        _statusMessage = "Moedas insuficientes — abra a loja.";
                        Show(AppScreen.Shop);
                    }
                });

                string videoLabel = videoReady ? "Continuar (vídeo)" : "Vídeo indisponível";
                AddButton(videoLabel, new Vector2(0, 40), () =>
                {
                    if (!(MonetizationHub.Instance?.Ads.IsRewardedReady ?? false))
                    {
                        _statusMessage = "Vídeo ainda carregando — tente de novo.";
                        Rebuild();
                        return;
                    }
                    _statusMessage = "Carregando vídeo…";
                    Rebuild();
                    MonetizationHub.Instance.Ads.ShowRewarded(ok =>
                    {
                        if (ok)
                            ResumeAfterDefeat();
                        else
                        {
                            _statusMessage = "Vídeo não concluído — tente outra opção.";
                            Rebuild();
                        }
                    });
                });

                AddButton("Tentar de novo (−1 vida)", new Vector2(0, -80), () =>
                {
                    _statusMessage = "";
                    TryStartLevel(levelId);
                });
                navY = -320f;
            }

            AddChromeButton("Mapa", new Vector2(0, navY), () => Show(AppScreen.LevelSelect), new Vector2(480, 96), ButtonStyle.Action);
            AddChromeButton("Início", new Vector2(0, navY - 140f), () => Show(AppScreen.Home), new Vector2(480, 96), ButtonStyle.Secondary);
        }

        void BuildShop()
        {
            var iap = MonetizationHub.Instance.Iap;
            var p = PlayerProgress.Instance;
            var data = p.Data;
            AddArtBackground(0.4f);
            AddPanel(new Vector2(0, 880), new Vector2(1000, 140), new Color(0.06f, 0.08f, 0.16f, 0.9f));
            AddLabel("Loja Lumina", 44, new Vector2(0, 920), Color.white);
            AddLabel($"Saldo: {data.Coins} moedas   ·   Vidas {data.Lives}/{data.MaxLives}", 22, new Vector2(0, 860), Color.white);
            AddLabel(iap.StatusText, 18, new Vector2(0, 820), new Color(0.75f, 0.85f, 1f));
            if (!string.IsNullOrEmpty(_statusMessage))
                AddLabel(_statusMessage, 20, new Vector2(0, 780), new Color(1f, 0.8f, 0.45f));
            AddChromeButton("Voltar", new Vector2(-420, 900), () => Show(AppScreen.Home), new Vector2(160, 72), ButtonStyle.Secondary);

            var content = CreateVerticalScroll(0.06f, 0.72f);
            bool dailyOffer = OfferService.ShouldShowDailyOffer(data, System.DateTime.UtcNow);
            bool starter = OfferService.ShouldShowStarterPack(data);

            AddShopHeader(content, "Com moedas");
            AddShopButton(content, $"+1 vida — {SoftShop.LifeCost} moedas", () =>
            {
                _statusMessage = SoftShop.TryBuyLife(p) ? "+1 vida!" : "Moedas insuficientes ou vidas cheias.";
                Rebuild();
            });
            AddShopButton(content, $"Encher vidas — {SoftShop.LivesFullCost} moedas", () =>
            {
                _statusMessage = SoftShop.TryBuyFullLives(p) ? "Vidas cheias!" : "Moedas insuficientes ou vidas cheias.";
                Rebuild();
            });
            AddShopButton(content, $"Martelo ×1 — {SoftShop.HammerCost} moedas", () =>
            {
                _statusMessage = SoftShop.TryBuyBooster(p, BoosterType.Hammer) ? "Martelo +1" : "Moedas insuficientes.";
                Rebuild();
            });
            AddShopButton(content, $"Troca ×1 — {SoftShop.SwapCost} moedas", () =>
            {
                _statusMessage = SoftShop.TryBuyBooster(p, BoosterType.Swap) ? "Troca +1" : "Moedas insuficientes.";
                Rebuild();
            });
            AddShopButton(content, $"Linha ×1 — {SoftShop.LineBlastCost} moedas", () =>
            {
                _statusMessage = SoftShop.TryBuyBooster(p, BoosterType.LineBlast) ? "Linha +1" : "Moedas insuficientes.";
                Rebuild();
            });

            AddShopHeader(content, "Compras reais (IAP)");
            if (dailyOffer)
            {
                AddShopButton(content, $"Oferta diária — {iap.GetPriceLabel(IapProductId.CoinsMedium)}", () =>
                    BuyIap(IapProductId.CoinsMedium, ok =>
                    {
                        if (ok) OfferService.MarkDailyClaimed(p, System.DateTime.UtcNow);
                    }));
            }
            if (starter)
            {
                AddShopButton(content, $"Pacote estreia — {iap.GetPriceLabel(IapProductId.StarterPack)}", () =>
                    BuyIap(IapProductId.StarterPack));
            }

            foreach (IapProductId id in System.Enum.GetValues(typeof(IapProductId)))
            {
                if (id == IapProductId.StarterPack) continue;
                if (dailyOffer && id == IapProductId.CoinsMedium) continue; // already listed as daily offer
                var captured = id;
                AddShopButton(content, $"{ProductLabel(id)} — {iap.GetPriceLabel(id)}", () => BuyIap(captured));
            }

            AddShopButton(content, "Restaurar compras", () =>
            {
                iap.RestorePurchases(ok =>
                {
                    _statusMessage = ok ? "Compras restauradas." : "Nada para restaurar / loja offline.";
                    Rebuild();
                });
            }, ButtonStyle.Secondary);
        }

        Transform CreateVerticalScroll(float anchorMinY, float anchorMaxY)
        {
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollGo.transform.SetParent(_root, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, anchorMinY);
            scrollRt.anchorMax = new Vector2(0.95f, anchorMaxY);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.55f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = true;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scrollGo.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = Vector2.zero;
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(20, 20, 20, 28);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            return content.transform;
        }

        void AddShopHeader(Transform parent, string text)
        {
            var go = new GameObject("Header", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 48;
            var label = CreateText(go.transform, text, 26);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.9f, 0.55f);
            label.fontStyle = FontStyle.Bold;
        }

        void AddShopButton(Transform parent, string text, UnityEngine.Events.UnityAction action, ButtonStyle style = ButtonStyle.Action)
        {
            var go = new GameObject(text.Length > 24 ? text.Substring(0, 24) : text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 88;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = style switch
            {
                ButtonStyle.Primary => new Color(0.78f, 0.55f, 0.18f, 1f),
                ButtonStyle.Active => new Color(0.85f, 0.62f, 0.2f, 1f),
                ButtonStyle.Secondary => new Color(0.28f, 0.3f, 0.4f, 1f),
                ButtonStyle.Info => new Color(0.22f, 0.55f, 0.58f, 1f),
                _ => new Color(0.28f, 0.38f, 0.72f, 1f)
            };
            go.GetComponent<Button>().onClick.AddListener(action);
            var label = CreateText(go.transform, text, 24);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
        }

        void AddPalaceProgressBar(Vector2 pos, Vector2 size, float fill01)
        {
            var bg = new GameObject("PalaceBarBg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(_root, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.sizeDelta = size;
            bgRt.anchoredPosition = pos;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = _whiteSprite;
            bgImg.color = new Color(0.12f, 0.14f, 0.22f, 0.95f);
            bgImg.raycastTarget = false;

            var fill = new GameObject("PalaceBarFill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(fill01), 1f);
            fillRt.offsetMin = new Vector2(3f, 3f);
            fillRt.offsetMax = new Vector2(-3f, -3f);
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = _whiteSprite;
            fillImg.color = new Color(0.95f, 0.78f, 0.35f, 1f);
            fillImg.raycastTarget = false;
        }

        void BuyIap(IapProductId id, System.Action<bool> extra = null)
        {
            var iap = MonetizationHub.Instance.Iap;
            if (!iap.IsReady)
            {
                _statusMessage = "Loja ainda conectando — tente em instantes.";
                Rebuild();
                return;
            }
            _statusMessage = "Processando compra…";
            Rebuild();
            iap.Purchase(id, ok =>
            {
                extra?.Invoke(ok);
                _statusMessage = ok
                    ? $"Compra ok: {ProductLabel(id)}"
                    : "Compra cancelada ou loja indisponível (sem cobrança falsa).";
                Rebuild();
            });
        }

        static string ProductLabel(IapProductId id) => id switch
        {
            IapProductId.CoinsSmall => "500 moedas",
            IapProductId.CoinsMedium => "1500 moedas",
            IapProductId.CoinsLarge => "5000 moedas",
            IapProductId.LivesRefill => "Recarregar vidas",
            IapProductId.BoosterPack => "Pacote de poderes",
            IapProductId.RemoveAds => "Remover anúncios",
            IapProductId.StarterPack => "Pacote estreia",
            _ => "Item"
        };

        void BuildOutOfLives()
        {
            AddBackground(new Color(0.12f, 0.05f, 0.08f));
            AddLabel("Sem vidas", 52, new Vector2(0, 300), Color.white);
            AddLifeRegenDisplay(new Vector2(0, 180), new Vector2(560, 80));
            bool videoReady = MonetizationHub.Instance?.Ads.IsRewardedReady ?? false;
            AddButton(videoReady ? "Assistir vídeo (+1 vida)" : "Vídeo indisponível", new Vector2(0, 60), () =>
            {
                if (!(MonetizationHub.Instance?.Ads.IsRewardedReady ?? false))
                {
                    _statusMessage = "Vídeo indisponível — compre vidas ou espere a regen.";
                    Rebuild();
                    return;
                }
                MonetizationHub.Instance.Ads.ShowRewarded(ok =>
                {
                    if (ok)
                    {
                        PlayerProgress.Instance.AddLives(1);
                        _statusMessage = "+1 vida!";
                        Show(AppScreen.Home);
                    }
                    else
                    {
                        _statusMessage = "Vídeo não concluído.";
                        Rebuild();
                    }
                });
            });
            AddButton($"+1 vida — {SoftShop.LifeCost} moedas", new Vector2(0, -60 + MenuBackUpShift), () =>
            {
                _statusMessage = SoftShop.TryBuyLife(PlayerProgress.Instance)
                    ? "+1 vida!"
                    : "Moedas insuficientes.";
                if (PlayerProgress.Instance.HasLife())
                    Show(AppScreen.Home);
                else
                    Rebuild();
            });
            AddButton("Loja (IAP / moedas)", new Vector2(0, -180 + MenuBackUpShift), () => Show(AppScreen.Shop));
            AddButton("Voltar", new Vector2(0, -300 + MenuBackUpShift), () => Show(AppScreen.Home));
        }

        // --- UI helpers ---

        enum ButtonStyle
        {
            Primary,
            Action,
            Active,
            Info,
            Secondary
        }

        void AddGearButton(Vector2 pos)
        {
            var go = new GameObject("SettingsGear", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(88, 88);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = new Color(0.22f, 0.38f, 0.72f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_boardBusy && _screen == AppScreen.Gameplay) return;
                _settingsOpen = true;
                if (_screen == AppScreen.Gameplay)
                {
                    _boardBusy = true;
                    _boardPresenter?.SetInputLocked(true);
                }
                SfxPlayer.Instance?.PlayClick();
                Rebuild();
            });

            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(go.transform, false);
            var srt = shine.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.1f, 0.55f);
            srt.anchorMax = new Vector2(0.9f, 0.92f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            var simg = shine.GetComponent<Image>();
            simg.sprite = _whiteSprite;
            simg.color = new Color(1f, 1f, 1f, 0.14f);
            simg.raycastTarget = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var irt = iconGo.GetComponent<RectTransform>();
            irt.sizeDelta = new Vector2(56, 56);
            irt.anchoredPosition = Vector2.zero;
            var iimg = iconGo.GetComponent<Image>();
            iimg.sprite = SettingsIcons.Gear;
            iimg.color = Color.white;
            iimg.raycastTarget = false;
            iimg.preserveAspect = true;
        }

        void BuildSettingsModal()
        {
            var p = PlayerProgress.Instance;
            if (p == null) return;

            var dimGo = new GameObject("SettingsDim", typeof(RectTransform), typeof(Image), typeof(Button));
            dimGo.transform.SetParent(_root, false);
            Stretch(dimGo.GetComponent<RectTransform>());
            var dimImg = dimGo.GetComponent<Image>();
            dimImg.sprite = _whiteSprite;
            dimImg.color = new Color(0f, 0f, 0f, 0.55f);
            dimGo.GetComponent<Button>().onClick.AddListener(CloseSettings);

            var panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_root, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.sizeDelta = new Vector2(720, 680);
            panelRt.anchoredPosition = Vector2.zero;
            var panelImg = panel.GetComponent<Image>();
            panelImg.sprite = _whiteSprite;
            panelImg.color = new Color(0.92f, 0.95f, 1f, 0.98f);

            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(panel.transform, false);
            Stretch(border.GetComponent<RectTransform>());
            var borderImg = border.GetComponent<Image>();
            borderImg.sprite = _whiteSprite;
            borderImg.color = new Color(0.45f, 0.72f, 0.95f, 1f);
            border.transform.SetAsFirstSibling();
            var inset = new GameObject("Inset", typeof(RectTransform), typeof(Image));
            inset.transform.SetParent(panel.transform, false);
            var insetRt = inset.GetComponent<RectTransform>();
            Stretch(insetRt);
            insetRt.offsetMin = new Vector2(10, 10);
            insetRt.offsetMax = new Vector2(-10, -10);
            inset.GetComponent<Image>().sprite = _whiteSprite;
            inset.GetComponent<Image>().color = new Color(0.94f, 0.97f, 1f, 1f);

            var header = new GameObject("Header", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(panel.transform, false);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.sizeDelta = new Vector2(0, 90);
            headerRt.anchoredPosition = Vector2.zero;
            header.GetComponent<Image>().sprite = _whiteSprite;
            header.GetComponent<Image>().color = new Color(0.16f, 0.28f, 0.55f, 1f);

            var title = CreateText(header.transform, "CONFIGURAÇÕES", 34);
            Stretch(title.rectTransform);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.fontStyle = FontStyle.Bold;

            var closeGo = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(panel.transform, false);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1, 1);
            closeRt.pivot = new Vector2(1, 1);
            closeRt.sizeDelta = new Vector2(72, 72);
            closeRt.anchoredPosition = new Vector2(-18, -10);
            closeGo.GetComponent<Image>().sprite = _whiteSprite;
            closeGo.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.4f, 1f);
            closeGo.GetComponent<Button>().onClick.AddListener(CloseSettings);
            var closeLabel = CreateText(closeGo.transform, "X", 32);
            Stretch(closeLabel.rectTransform);
            closeLabel.alignment = TextAnchor.MiddleCenter;
            closeLabel.color = Color.white;
            closeLabel.fontStyle = FontStyle.Bold;

            AddSettingsToggle(panel.transform, new Vector2(-210, 80), SettingsIcons.Speaker(p.Data.SfxOn), p.Data.SfxOn, on =>
            {
                p.SetSfxOn(on);
                SfxPlayer.Instance?.PlayClick();
                Rebuild();
            });
            AddSettingsToggle(panel.transform, new Vector2(0, 80), SettingsIcons.Music(p.Data.MusicOn), p.Data.MusicOn, on =>
            {
                p.SetMusicOn(on);
                Rebuild();
            });
            AddSettingsToggle(panel.transform, new Vector2(210, 80), SettingsIcons.Vibrate(p.Data.VibrateOn), p.Data.VibrateOn, on =>
            {
                p.SetVibrateOn(on);
                if (on) Haptics.PulseMove();
                Rebuild();
            });

            var hint = CreateText(panel.transform, "Sons   ·   Música   ·   Vibração", 22);
            hint.rectTransform.anchoredPosition = new Vector2(0, -70);
            hint.rectTransform.sizeDelta = new Vector2(600, 40);
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = new Color(0.25f, 0.3f, 0.4f, 1f);

            // Início — leaves gameplay (costs a life if mid-level) and returns home.
            var homeGo = new GameObject("Inicio", typeof(RectTransform), typeof(Image), typeof(Button));
            homeGo.transform.SetParent(panel.transform, false);
            var homeRt = homeGo.GetComponent<RectTransform>();
            homeRt.sizeDelta = new Vector2(420, 88);
            homeRt.anchoredPosition = new Vector2(0, -160);
            homeGo.GetComponent<Image>().sprite = _whiteSprite;
            homeGo.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.42f, 1f);
            homeGo.GetComponent<Button>().onClick.AddListener(GoHomeFromSettings);
            var homeLabel = CreateText(homeGo.transform, "Início", 30);
            Stretch(homeLabel.rectTransform);
            homeLabel.alignment = TextAnchor.MiddleCenter;
            homeLabel.color = Color.white;
            homeLabel.fontStyle = FontStyle.Bold;

            var ver = CreateText(panel.transform, $"v{Application.version}", 20);
            ver.rectTransform.anchoredPosition = new Vector2(0, -260);
            ver.rectTransform.sizeDelta = new Vector2(200, 36);
            ver.alignment = TextAnchor.MiddleCenter;
            ver.color = new Color(0.85f, 0.45f, 0.2f, 1f);
        }

        void GoHomeFromSettings()
        {
            _settingsOpen = false;
            SfxPlayer.Instance?.PlayClick();
            if (_screen == AppScreen.Gameplay && _session != null && !_session.IsWon)
            {
                SpendLifeOnFailureOnce();
                _session = null;
                _boardBusy = false;
                _boardPresenter?.SetInputLocked(false);
            }
            Show(AppScreen.Home);
        }

        void AddSettingsToggle(Transform parent, Vector2 pos, Sprite icon, bool on, System.Action<bool> onToggle)
        {
            var go = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 150);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = on
                ? new Color(0.55f, 0.35f, 0.85f, 1f)
                : new Color(0.55f, 0.58f, 0.65f, 1f);
            go.GetComponent<Button>().onClick.AddListener(() => onToggle(!on));

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var irt = iconGo.GetComponent<RectTransform>();
            irt.sizeDelta = new Vector2(84, 84);
            irt.anchoredPosition = new Vector2(0, 6);
            var iimg = iconGo.GetComponent<Image>();
            iimg.sprite = icon;
            iimg.color = Color.white;
            iimg.raycastTarget = false;
            iimg.preserveAspect = true;
        }

        void CloseSettings()
        {
            _settingsOpen = false;
            if (_screen == AppScreen.Gameplay && !_boardBusy)
            { }
            if (_screen == AppScreen.Gameplay)
            {
                // Unlock only if not mid-resolve animation
                // _boardBusy may still be true from opening settings; clear if idle resolve not running
                _boardBusy = false;
                _boardPresenter?.SetInputLocked(false);
            }
            SfxPlayer.Instance?.PlayClick();
            Rebuild();
        }

        void AddBackground(Color color)
        {
            var go = new GameObject("BG", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            Stretch(go.GetComponent<RectTransform>());
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
        }

        void AddArtBackground(float dimAlpha)
        {
            var bg = new GameObject("ArtBG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(_root, false);
            Stretch(bg.GetComponent<RectTransform>());
            var img = bg.GetComponent<Image>();
            img.sprite = ArtCatalog.Background ?? _whiteSprite;
            img.preserveAspect = false;
            img.color = Color.white;
            img.raycastTarget = false;

            var dim = new GameObject("ArtDim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(_root, false);
            Stretch(dim.GetComponent<RectTransform>());
            var dimImg = dim.GetComponent<Image>();
            dimImg.sprite = _whiteSprite;
            dimImg.color = new Color(0.02f, 0.03f, 0.1f, Mathf.Clamp01(dimAlpha));
            dimImg.raycastTarget = false;
        }

        void AddPanel(Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
            img.raycastTarget = false;
        }

        void AddHudChip(string text, Vector2 pos, Vector2 size, Color bg)
            => AddHudChipText(text, pos, size, bg);

        Text AddHudChipText(string text, Vector2 pos, Vector2 size, Color bg)
        {
            var go = new GameObject("Chip", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = bg;
            img.raycastTarget = false;
            var label = CreateText(go.transform, text, 22);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
            return label;
        }

        void AddLifeRegenDisplay(Vector2 pos, Vector2 size)
        {
            var p = PlayerProgress.Instance;
            bool full = p != null && p.Data.Lives >= p.Data.MaxLives;
            int sec = p != null ? p.SecondsToNextLife() : 0;
            string text = p != null
                ? FormatLifeRegenHud(p, sec)
                : "Vidas";
            var bg = full
                ? new Color(0.18f, 0.42f, 0.28f, 0.95f)
                : new Color(0.55f, 0.18f, 0.28f, 0.95f);
            _lifeRegenHud = AddHudChipText(text, pos, size, bg);
            _lastLifeRegenShown = full ? -1 : sec;
            if (_lifeRegenHud != null)
            {
                _lifeRegenHud.fontSize = size.y >= 70 ? 24 : 18;
                _lifeRegenHud.lineSpacing = 0.85f;
            }
        }

        void AddLabel(string text, int size, Vector2 pos, Color color)
        {
            var t = CreateText(_root, text, size);
            t.color = color;
            t.rectTransform.anchoredPosition = pos;
            t.rectTransform.sizeDelta = new Vector2(1000, size + 28);
            t.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// iPad / tablets are wider than the 1080×1920 phone layout, so chrome at ±900
        /// gets clipped. Shrink the whole UI a bit and nudge it slightly downward.
        /// </summary>
        void ApplyTabletScreenFit()
        {
            if (_root == null || !IsTabletLikeDisplay())
                return;

            // ~10% smaller so Home / Gameplay / Shop fit inside 4:3 without cutting Map / headers.
            const float tabletScale = 0.90f;
            _root.localScale = new Vector3(tabletScale, tabletScale, 1f);

            // "Abaixar" a composição: um pouco mais de margem no topo do que na base.
            const float sidePad = 20f;
            const float topPad = 40f;
            const float bottomPad = 16f;
            _root.offsetMin = new Vector2(sidePad, bottomPad);
            _root.offsetMax = new Vector2(-sidePad, -topPad);

            Debug.Log($"[LuminaMatch] Tablet/iPad UI fit: scale={tabletScale}, pads=({sidePad},{topPad},{bottomPad})");
        }

        static bool IsTabletLikeDisplay()
        {
#if UNITY_IOS
            string model = SystemInfo.deviceModel ?? "";
            if (model.IndexOf("iPad", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
#endif
            float w = Mathf.Max(1f, Screen.width);
            float h = Mathf.Max(1f, Screen.height);
            float shortSide = Mathf.Min(w, h);
            float longSide = Mathf.Max(w, h);
            float aspect = shortSide / longSide; // portrait: width/height
            // Phones ~0.45–0.5; iPads ~0.70–0.75.
            if (aspect >= 0.62f)
                return true;

            float dpi = Screen.dpi > 20f ? Screen.dpi : 160f;
            float diagInches = Mathf.Sqrt(w * w + h * h) / dpi;
            return diagInches >= 7.0f;
        }

        /// <summary>Canvas size in scaler units (matches CanvasScaler Scale With Screen Size).</summary>
        void GetGameplayCanvasSize(out float canvasW, out float canvasH)
        {
            const float refW = 1080f;
            const float refH = 1920f;
            float match = IsTabletLikeDisplay() ? 0.85f : 0.5f;
            float sw = Mathf.Max(1f, Screen.width);
            float sh = Mathf.Max(1f, Screen.height);
            float logW = Mathf.Log(sw / refW);
            float logH = Mathf.Log(sh / refH);
            float scale = Mathf.Exp(Mathf.Lerp(logW, logH, match));
            canvasW = sw / scale;
            canvasH = sh / scale;

            if (_root != null && _root.rect.width > 100f && _root.rect.height > 100f)
            {
                // Live rect in layout space (localScale is visual-only; do not divide).
                canvasW = _root.rect.width;
                canvasH = _root.rect.height;
            }
        }

        void AddVersionLabel(Vector2 pos, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var t = CreateText(_root, $"v{Application.version}", 18);
            t.color = new Color(1f, 1f, 1f, 0.55f);
            t.rectTransform.anchoredPosition = pos;
            t.rectTransform.sizeDelta = new Vector2(200, 36);
            t.alignment = align;
            t.raycastTarget = false;
        }

        void AddButton(string text, Vector2 pos, UnityEngine.Events.UnityAction action, Vector2? size = null)
            => AddChromeButton(text, pos, action, size ?? new Vector2(520, 100), ButtonStyle.Primary);

        void AddChromeButton(string text, Vector2 pos, UnityEngine.Events.UnityAction action, Vector2 size, ButtonStyle style)
        {
            var go = new GameObject(text.Replace("\n", " "), typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = style switch
            {
                ButtonStyle.Primary => new Color(0.78f, 0.55f, 0.18f, 1f),
                ButtonStyle.Action => new Color(0.28f, 0.38f, 0.72f, 1f),
                ButtonStyle.Active => new Color(0.85f, 0.62f, 0.2f, 1f),
                ButtonStyle.Info => new Color(0.22f, 0.55f, 0.58f, 1f),
                ButtonStyle.Secondary => new Color(0.28f, 0.3f, 0.4f, 1f),
                _ => new Color(0.35f, 0.45f, 0.85f, 1f)
            };
            go.GetComponent<Button>().onClick.AddListener(action);

            // Soft top highlight edge
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(go.transform, false);
            var srt = shine.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.06f, 0.55f);
            srt.anchorMax = new Vector2(0.94f, 0.92f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            var simg = shine.GetComponent<Image>();
            simg.sprite = _whiteSprite;
            simg.color = new Color(1f, 1f, 1f, 0.12f);
            simg.raycastTarget = false;

            bool multi = text.Contains("\n");
            var label = CreateText(go.transform, text, multi ? 22 : 26);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
            label.lineSpacing = 0.9f;
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
