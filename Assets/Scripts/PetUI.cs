using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PetUI : MonoBehaviour
{
    private static readonly Color BarHunger = new Color(0.95f, 0.62f, 0.3f, 1f);
    private static readonly Color BarHappy = new Color(0.42f, 0.85f, 0.5f, 1f);
    private static readonly Color BarEnergy = new Color(0.4f, 0.65f, 0.95f, 1f);
    private static readonly Color BgDark = new Color(0.09f, 0.11f, 0.16f, 0.94f);
    private static readonly Color BarBg = new Color(0.16f, 0.2f, 0.28f, 0.9f);
    private static readonly Color TextWhite = new Color(0.96f, 0.96f, 0.96f, 1f);
    private static readonly Color FeedColor = new Color(0.85f, 0.55f, 0.3f, 1f);
    private static readonly Color PlayColor = new Color(0.42f, 0.75f, 0.45f, 1f);
    private static readonly Color SleepColor = new Color(0.45f, 0.55f, 0.85f, 1f);

    private Pet pet;
    private PetSave petSave;
    private Font font;

    private Image nightOverlay;
    private Text nameText;
    private Text stateText;
    private Text dayText;
    private Text timeText;
    private Text messageText;
    private Image hungerFill;
    private Image happinessFill;
    private Image energyFill;
    private Text hungerValue;
    private Text happinessValue;
    private Text energyValue;

    private Button feedButton;
    private Button playButton;
    private Button sleepButton;
    private Text feedLabel;
    private Text playLabel;
    private Text sleepLabel;
    private Text feedCd;
    private Text playCd;
    private Text sleepCd;
    private Text musicLabel;

    private GameObject startPanel;
    private GameObject gameOverPanel;
    private GameObject victoryPanel;
    private InputField nameInput;
    private Text overReason;
    private Text overStats;
    private Text victoryText;

    private float messageTimer;
    private bool resumeCheckDone;

    private void Awake()
    {
        pet = GetComponent<Pet>();
        petSave = GetComponent<PetSave>();
        font = LoadFont();
        BuildCanvas();
        BuildHud();
        BuildStartPanel();
        BuildGameOverPanel();
        BuildVictoryPanel();
        Subscribe();
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
    }

    // ------------------------------------------------------------------ setup

    private static Font LoadFont()
    {
        try
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) return f;
        }
        catch (System.Exception) { }
        try
        {
            Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f != null) return f;
        }
        catch (System.Exception) { }
        return null;
    }

    private void BuildCanvas()
    {
        var go = new GameObject("CreatureCareCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var overlay = NewUI("NightOverlay", go.transform);
        Stretch(overlay);
        nightOverlay = overlay.gameObject.AddComponent<Image>();
        nightOverlay.color = new Color(0.05f, 0.07f, 0.2f, 0f);
        nightOverlay.raycastTarget = false;

        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }

    private void BuildHud()
    {
        RectTransform info = NewUI("Info", nightOverlay.transform.parent);
        info.anchorMin = info.anchorMax = new Vector2(0f, 1f);
        info.pivot = new Vector2(0f, 1f);
        info.anchoredPosition = new Vector2(24f, -20f);
        info.sizeDelta = new Vector2(500f, 150f);

        nameText = MakeText("Name", info, pet.PetName, 34, TextWhite, TextAnchor.MiddleLeft);
        Anchored(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(500f, 44f));
        AddOutline(nameText);

        stateText = MakeText("State", info, "Content", 22, TextWhite, TextAnchor.MiddleLeft);
        Anchored(stateText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -50f), new Vector2(500f, 30f));
        AddOutline(stateText);

        dayText = MakeText("Day", info, "Day 1 / 5", 22, TextWhite, TextAnchor.MiddleLeft);
        Anchored(dayText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -88f), new Vector2(500f, 30f));
        AddOutline(dayText);

        RectTransform time = NewUI("Time", nightOverlay.transform.parent);
        time.anchorMin = time.anchorMax = new Vector2(0.5f, 1f);
        time.pivot = new Vector2(0.5f, 1f);
        time.anchoredPosition = new Vector2(0f, -24f);
        time.sizeDelta = new Vector2(160f, 36f);
        timeText = MakeText("TimeText", time, "Day", 24, new Color(1f, 0.88f, 0.5f, 1f), TextAnchor.MiddleCenter);
        Stretch(timeText.rectTransform);
        AddOutline(timeText);

        RectTransform stats = NewUI("Stats", nightOverlay.transform.parent);
        stats.anchorMin = stats.anchorMax = new Vector2(1f, 1f);
        stats.pivot = new Vector2(1f, 1f);
        stats.anchoredPosition = new Vector2(-24f, -20f);
        stats.sizeDelta = new Vector2(400f, 210f);

        MakeStatRow(stats, "Hunger", BarHunger, new Vector2(0f, 0f), out hungerFill, out hungerValue);
        MakeStatRow(stats, "Happiness", BarHappy, new Vector2(0f, -72f), out happinessFill, out happinessValue);
        MakeStatRow(stats, "Energy", BarEnergy, new Vector2(0f, -144f), out energyFill, out energyValue);

        RectTransform msg = NewUI("Message", nightOverlay.transform.parent);
        msg.anchorMin = msg.anchorMax = new Vector2(0.5f, 1f);
        msg.pivot = new Vector2(0.5f, 1f);
        msg.anchoredPosition = new Vector2(0f, -258f);
        msg.sizeDelta = new Vector2(1000f, 54f);
        messageText = MakeText("Msg", msg, "", 25, TextWhite, TextAnchor.MiddleCenter);
        Stretch(messageText.rectTransform);
        AddOutline(messageText);

        RectTransform bar = NewUI("ActionBar", nightOverlay.transform.parent);
        bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.anchoredPosition = new Vector2(0f, 34f);
        bar.sizeDelta = new Vector2(820f, 120f);

        feedButton = MakeButton("FeedButton", bar, "Feed", FeedColor, out feedLabel);
        feedButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f, 0f);
        feedCd = MakeText("Cd", feedButton.transform, "", 16, TextWhite, TextAnchor.MiddleCenter);
        Anchored(feedCd.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(160f, 20f));

        playButton = MakeButton("PlayButton", bar, "Play", PlayColor, out playLabel);
        playButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        playCd = MakeText("Cd", playButton.transform, "", 16, TextWhite, TextAnchor.MiddleCenter);
        Anchored(playCd.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(160f, 20f));

        sleepButton = MakeButton("SleepButton", bar, "Sleep", SleepColor, out sleepLabel);
        sleepButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(260f, 0f);
        sleepCd = MakeText("Cd", sleepButton.transform, "", 16, TextWhite, TextAnchor.MiddleCenter);
        Anchored(sleepCd.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(160f, 20f));

        feedButton.onClick.AddListener(() => pet.TryFeed());
        playButton.onClick.AddListener(() => pet.TryPlay());
        sleepButton.onClick.AddListener(() => pet.TrySleepToggle());

        RectTransform musicBtnRt = NewUI("MusicButton", nightOverlay.transform.parent);
        musicBtnRt.anchorMin = musicBtnRt.anchorMax = new Vector2(0f, 1f);
        musicBtnRt.pivot = new Vector2(0f, 1f);
        musicBtnRt.anchoredPosition = new Vector2(24f, -180f);
        musicBtnRt.sizeDelta = new Vector2(170f, 54f);
        Image musicImg = musicBtnRt.gameObject.AddComponent<Image>();
        musicImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);
        Button musicButton = musicBtnRt.gameObject.AddComponent<Button>();
        musicButton.targetGraphic = musicImg;
        musicButton.onClick.AddListener(ToggleMusic);
        musicLabel = MakeText("Label", musicBtnRt, "Mute", 22, TextWhite, TextAnchor.MiddleCenter);
        Anchored(musicLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 40f));
        AddOutline(musicLabel);
    }

    private void ToggleMusic()
    {
        BackgroundMusic bgm = BackgroundMusic.Instance;
        if (bgm != null) bgm.Toggle();
    }

    private void MakeStatRow(Transform parent, string label, Color fillColor, Vector2 pos, out Image fill, out Text valueText)
    {
        RectTransform row = NewUI("Row_" + label, parent);
        row.sizeDelta = new Vector2(400f, 52f);
        row.anchoredPosition = pos;

        Text labelText = MakeText("Label", row, label, 22, TextWhite, TextAnchor.MiddleLeft);
        Anchored(labelText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(120f, 34f));

        RectTransform bg = NewUI("Bg", row);
        Anchored(bg, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(135f, 0f), new Vector2(165f, 20f));
        Image bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = BarBg;

        RectTransform fillRt = NewUI("Fill", bg);
        Stretch(fillRt);
        fill = fillRt.gameObject.AddComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0.8f;
        fill.color = fillColor;
        fill.raycastTarget = false;

        valueText = MakeText("Value", row, "80", 20, TextWhite, TextAnchor.MiddleRight);
        Anchored(valueText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(90f, 30f));
    }

    private Button MakeButton(string name, Transform parent, string label, Color color, out Text labelText)
    {
        RectTransform rt = NewUI(name, parent);
        rt.sizeDelta = new Vector2(190f, 92f);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.8f;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.55f);
        colors.colorMultiplier = 1f;
        btn.colors = colors;

        labelText = MakeText("Label", rt, label, 28, TextWhite, TextAnchor.MiddleCenter);
        Anchored(labelText.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(170f, 46f));
        AddOutline(labelText);
        return btn;
    }

    private void BuildStartPanel()
    {
        startPanel = NewUI("StartPanel", nightOverlay.transform.parent).gameObject;
        Image dim = startPanel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        Stretch(dim.rectTransform);

        RectTransform card = NewUI("Card", startPanel.transform);
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(640f, 500f);
        card.anchoredPosition = new Vector2(0f, 0f);
        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.color = BgDark;

        Text title = MakeText("Title", card, "CREATURE CARE", 44, TextWhite, TextAnchor.MiddleCenter);
        Anchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(560f, 60f));
        AddOutline(title);

        Text subtitle = MakeText("Sub", card, "A tiny friend who depends on you.", 22, new Color(0.85f, 0.87f, 0.9f, 1f), TextAnchor.MiddleCenter);
        Anchored(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(560f, 40f));

        Text label = MakeText("NameLabel", card, "Name your pet:", 24, TextWhite, TextAnchor.MiddleCenter);
        Anchored(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -175f), new Vector2(560f, 36f));

        RectTransform inputBg = NewUI("InputBg", card);
        Anchored(inputBg, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(420f, 64f));
        Image inputImage = inputBg.gameObject.AddComponent<Image>();
        inputImage.color = BarBg;

        nameInput = inputBg.gameObject.AddComponent<InputField>();
        Text display = MakeText("Text", inputBg, "", 26, TextWhite, TextAnchor.MiddleCenter);
        Stretch(display.rectTransform);
        display.raycastTarget = false;
        Text placeholder = MakeText("Placeholder", inputBg, "Mochi", 26, new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleCenter);
        Stretch(placeholder.rectTransform);
        placeholder.raycastTarget = false;
        nameInput.textComponent = display;
        nameInput.placeholder = placeholder;
        nameInput.lineType = InputField.LineType.SingleLine;
        nameInput.characterLimit = 16;
        nameInput.text = "Mochi";

        Button start = MakeButton("Start", card, "Start", PlayColor, out Text startLabel);
        start.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -350f);
        start.onClick.AddListener(OnStartClicked);
    }

    private void BuildGameOverPanel()
    {
        gameOverPanel = BuildCenteredPanel("GameOverPanel", out RectTransform card);
        Text title = MakeText("Title", card, "GAME OVER", 46, new Color(0.95f, 0.4f, 0.4f, 1f), TextAnchor.MiddleCenter);
        Anchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(560f, 60f));

        overReason = MakeText("Reason", card, "", 24, TextWhite, TextAnchor.MiddleCenter);
        Anchored(overReason.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(520f, 60f));

        overStats = MakeText("Stats", card, "", 22, new Color(0.85f, 0.87f, 0.9f, 1f), TextAnchor.MiddleCenter);
        Anchored(overStats.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(520f, 90f));

        Button restart = MakeButton("Restart", card, "Start Over", FeedColor, out Text restartLabel);
        restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -360f);
        restart.onClick.AddListener(RestartGame);
    }

    private void BuildVictoryPanel()
    {
        victoryPanel = BuildCenteredPanel("VictoryPanel", out RectTransform card);
        Text title = MakeText("Title", card, "GOAL REACHED!", 44, new Color(0.5f, 0.9f, 0.6f, 1f), TextAnchor.MiddleCenter);
        Anchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(560f, 60f));

        victoryText = MakeText("Text", card, "", 26, TextWhite, TextAnchor.MiddleCenter);
        Anchored(victoryText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(520f, 80f));

        Button cont = MakeButton("Continue", card, "Keep Going", PlayColor, out Text contLabel);
        cont.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -320f);
        cont.onClick.AddListener(() => victoryPanel.SetActive(false));
    }

    private GameObject BuildCenteredPanel(string name, out RectTransform card)
    {
        GameObject panel = NewUI(name, nightOverlay.transform.parent).gameObject;
        Image dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);
        Stretch(dim.rectTransform);

        card = NewUI("Card", panel.transform);
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(620f, 480f);
        card.anchoredPosition = Vector2.zero;
        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.color = BgDark;
        return panel;
    }

    private void Subscribe()
    {
        if (pet == null) return;
        pet.Message += OnMessage;
        pet.StateChanged += OnStateChanged;
        pet.GameOver += OnGameOver;
        pet.Victory += OnVictory;
        pet.DayChanged += OnDayChanged;
    }

    private void OnDestroy()
    {
        if (pet == null) return;
        pet.Message -= OnMessage;
        pet.StateChanged -= OnStateChanged;
        pet.GameOver -= OnGameOver;
        pet.Victory -= OnVictory;
        pet.DayChanged -= OnDayChanged;
    }

    // ------------------------------------------------------------------ events

    private void OnStartClicked()
    {
        string name = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text) ? nameInput.text.Trim() : "Mochi";
        pet.BeginGame(name);
        nameText.text = pet.PetName;
        startPanel.SetActive(false);
    }

    private void OnMessage(string msg)
    {
        messageText.text = msg;
        messageTimer = 2.2f;
    }

    private void OnStateChanged(PetState previous, PetState next)
    {
        stateText.text = StateDisplay(next);
    }

    private void OnGameOver()
    {
        overReason.text = pet.PetName + " passed away from " + pet.DeathReason + ".";
        overStats.text = "Survived " + pet.Day + " day(s)\nHunger " + Mathf.RoundToInt(pet.Hunger)
            + "  |  Happiness " + Mathf.RoundToInt(pet.Happiness)
            + "  |  Energy " + Mathf.RoundToInt(pet.Energy);
        gameOverPanel.SetActive(true);
    }

    private void OnVictory()
    {
        victoryText.text = "You cared for " + pet.PetName + " for " + pet.daysToWin + " days!";
        victoryPanel.SetActive(true);
    }

    private void OnDayChanged(int day)
    {
        dayText.text = "Day " + day + " / " + pet.daysToWin;
    }

    private void RestartGame()
    {
        if (petSave != null) petSave.ClearSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ------------------------------------------------------------------ per frame

    private void Update()
    {
        if (pet == null) return;

        if (!resumeCheckDone)
        {
            resumeCheckDone = true;
            if (pet.Started)
            {
                startPanel.SetActive(false);
                nameText.text = pet.PetName;
                messageText.text = "Welcome back, " + pet.PetName + "!";
                messageTimer = 2.5f;
            }
        }

        UpdateHud();
        UpdateNightOverlay();

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) messageText.text = "";
        }

        HandleShortcuts();
    }

    private void UpdateHud()
    {
        nameText.text = pet.PetName;
        stateText.text = StateDisplay(pet.State);
        dayText.text = "Day " + pet.Day + " / " + pet.daysToWin;
        timeText.text = pet.IsNight ? "Night" : "Day";
        timeText.color = pet.IsNight ? new Color(0.55f, 0.65f, 1f, 1f) : new Color(1f, 0.88f, 0.5f, 1f);

        hungerFill.fillAmount = pet.Hunger / pet.maxStat;
        happinessFill.fillAmount = pet.Happiness / pet.maxStat;
        energyFill.fillAmount = pet.Energy / pet.maxStat;
        hungerValue.text = Mathf.RoundToInt(pet.Hunger).ToString();
        happinessValue.text = Mathf.RoundToInt(pet.Happiness).ToString();
        energyValue.text = Mathf.RoundToInt(pet.Energy).ToString();

        bool asleep = pet.IsSleeping;
        bool playing = pet.IsPlaying;

        feedCd.text = pet.FeedCooldownRemaining > 0f ? pet.FeedCooldownRemaining.ToString("0.0") + "s" : "";
        feedButton.interactable = pet.Started && !pet.IsGameOver && pet.FeedCooldownRemaining <= 0f && !asleep && !playing;

        playCd.text = pet.PlayCooldownRemaining > 0f ? pet.PlayCooldownRemaining.ToString("0.0") + "s" : "";
        playButton.interactable = pet.Started && !pet.IsGameOver && pet.PlayCooldownRemaining <= 0f && !asleep && !playing;

        sleepCd.text = pet.SleepCooldownRemaining > 0f ? pet.SleepCooldownRemaining.ToString("0.0") + "s" : "";
        sleepButton.interactable = pet.Started && !pet.IsGameOver && pet.SleepCooldownRemaining <= 0f && !playing;
        sleepLabel.text = asleep ? "Wake up" : "Sleep";

        if (musicLabel != null)
        {
            BackgroundMusic bgm = BackgroundMusic.Instance;
            musicLabel.text = bgm != null && bgm.IsMuted ? "Unmute" : "Mute";
        }
    }

    private void UpdateNightOverlay()
    {
        float p = pet.DayProgress;
        float alpha = 0f;
        if (p >= pet.nightStartFraction && p <= pet.nightEndFraction)
        {
            float t = (p - pet.nightStartFraction) / (pet.nightEndFraction - pet.nightStartFraction);
            alpha = Mathf.Sin(t * Mathf.PI) * 0.32f;
        }
        Color c = nightOverlay.color;
        c.a = Mathf.Lerp(c.a, alpha, Time.deltaTime * 4f);
        nightOverlay.color = c;
    }

    private void HandleShortcuts()
    {
        if (startPanel.activeSelf || gameOverPanel.activeSelf || victoryPanel.activeSelf) return;
        if (Keyboard.current == null) return;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame) pet.TryFeed();
        if (Keyboard.current.pKey.wasPressedThisFrame) pet.TryPlay();
        if (Keyboard.current.sKey.wasPressedThisFrame) pet.TrySleepToggle();
        if (Keyboard.current.mKey.wasPressedThisFrame) ToggleMusic();
    }

    private static string StateDisplay(PetState state)
    {
        switch (state)
        {
            case PetState.Happy: return "Happy";
            case PetState.Hungry: return "Hungry";
            case PetState.Sad: return "Sad";
            case PetState.Tired: return "Tired";
            case PetState.Sick: return "Sick";
            case PetState.Sleeping: return "Sleeping";
            case PetState.Playing: return "Playing";
            case PetState.Dead: return "Gone";
            default: return "Content";
        }
    }

    // ------------------------------------------------------------------ helpers

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private Text MakeText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor align)
    {
        RectTransform rt = NewUI(name, parent);
        Text txt = rt.gameObject.AddComponent<Text>();
        txt.font = font;
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = align;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;
        return txt;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Anchored(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    private static void AddOutline(Text t)
    {
        if (t == null) return;
        Outline o = t.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.65f);
        o.effectDistance = new Vector2(1.4f, -1.4f);
    }
}
