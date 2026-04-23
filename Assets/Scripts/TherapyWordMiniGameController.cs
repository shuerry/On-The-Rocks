using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TherapyWordMiniGameController : MonoBehaviour
{
    private const string MinigameSceneName = "Therapy Word MiniGame";
    private const string CompletionSceneName = "Therapy Scene WordCloud Ending";
    private static readonly Vector2 WordButtonSize = new Vector2(220f, 64f);

    [SerializeField] private int requiredSelections = 3;
    [SerializeField] private int minSliderValue = 1;
    [SerializeField] private int maxSliderValue = 10;

    private readonly string[] availableWords =
    {
        "Connected",
        "Whole",
        "Freedom",
        "Lighter",
        "Safe",
        "Chosen",
        "Enough",
        "Team",
        "Steady",
        "Seen",
        "Heavy",
        "Alone"
    };

    private readonly List<string> selectedWords = new List<string>();
    private readonly Dictionary<string, int> ratings = new Dictionary<string, int>();
    private readonly Dictionary<string, RectTransform> wordRects = new Dictionary<string, RectTransform>();
    private readonly Dictionary<string, Vector2> basePositions = new Dictionary<string, Vector2>();
    private readonly Dictionary<string, float> floatSpeeds = new Dictionary<string, float>();
    private readonly Dictionary<string, float> floatPhases = new Dictionary<string, float>();
    private readonly List<Vector2> edgeSlots = new List<Vector2>
    {
        new Vector2(0.14f, 0.72f),
        new Vector2(0.14f, 0.56f),
        new Vector2(0.14f, 0.40f),
        new Vector2(0.86f, 0.72f),
        new Vector2(0.86f, 0.56f),
        new Vector2(0.86f, 0.40f),
        new Vector2(0.25f, 0.24f),
        new Vector2(0.40f, 0.20f),
        new Vector2(0.60f, 0.20f),
        new Vector2(0.75f, 0.24f),
        new Vector2(0.30f, 0.72f),
        new Vector2(0.70f, 0.72f)
    };

    public static IReadOnlyList<string> LastSelectedWords => lastSelectedWords;
    public static IReadOnlyDictionary<string, int> LastRatings => lastRatings;

    private static List<string> lastSelectedWords = new List<string>();
    private static Dictionary<string, int> lastRatings = new Dictionary<string, int>();

    private Canvas rootCanvas;
    private LevelManager levelManager;
    private RectTransform wordsPanel;
    private RectTransform ratingPanel;
    private Button confirmSelectionButton;
    private Button submitRatingsButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != MinigameSceneName)
            return;

        if (FindFirstObjectByType<TherapyWordMiniGameController>() != null)
            return;

        GameObject host = new GameObject("TherapyWordMiniGameController");
        host.AddComponent<TherapyWordMiniGameController>();
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != MinigameSceneName)
        {
            Destroy(gameObject);
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        levelManager = FindFirstObjectByType<LevelManager>();

        EnsureEventSystem();
        EnsureCanvas();
        BuildWordSelectionUI();
    }

    private void Update()
    {
        if (wordsPanel == null || !wordsPanel.gameObject.activeSelf)
            return;

        float t = Time.time;
        foreach (var pair in wordRects)
        {
            string word = pair.Key;
            RectTransform rt = pair.Value;
            Vector2 basePos = basePositions[word];
            float yOffset = Mathf.Sin(t * floatSpeeds[word] + floatPhases[word]) * 10f;
            rt.anchoredPosition = new Vector2(basePos.x, basePos.y + yOffset);
        }
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();
    }

    private void EnsureCanvas()
    {
        rootCanvas = FindFirstObjectByType<Canvas>();

        if (rootCanvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            rootCanvas = canvasGo.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }
    }

    private void BuildWordSelectionUI()
    {
        wordsPanel = CreatePanel("WordsPanel", rootCanvas.transform as RectTransform, new Color(0f, 0f, 0f, 0f));
        StretchRect(wordsPanel);

        RectTransform heading = CreateText(
            wordsPanel,
            "Heading",
            "Choose 3 words that match how you feel right now",
            34,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.93f, 0.95f, 0.89f));

        heading.anchorMin = new Vector2(0.1f, 0.84f);
        heading.anchorMax = new Vector2(0.9f, 0.96f);
        heading.offsetMin = Vector2.zero;
        heading.offsetMax = Vector2.zero;

        RectTransform sub = CreateText(
            wordsPanel,
            "Subheading",
            "Selected: 0 / 3",
            24,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.82f, 0.87f, 0.8f));

        sub.anchorMin = new Vector2(0.2f, 0.77f);
        sub.anchorMax = new Vector2(0.8f, 0.84f);
        sub.offsetMin = Vector2.zero;
        sub.offsetMax = Vector2.zero;

        System.Random rng = new System.Random();
        List<Vector2> shuffledSlots = new List<Vector2>(edgeSlots);
        Shuffle(shuffledSlots, rng);

        for (int i = 0; i < availableWords.Length; i++)
        {
            string word = availableWords[i];
            Button wordButton = CreateWordButton(wordsPanel, word);
            RectTransform rt = wordButton.GetComponent<RectTransform>();

            Vector2 slot = shuffledSlots[i % shuffledSlots.Count];
            float x = slot.x * rootCanvas.pixelRect.width;
            float y = slot.y * rootCanvas.pixelRect.height;

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = WordButtonSize;
            rt.anchoredPosition = new Vector2(x, y);

            wordRects[word] = rt;
            basePositions[word] = rt.anchoredPosition;
            floatSpeeds[word] = 0.6f + (float)rng.NextDouble() * 0.8f;
            floatPhases[word] = (float)rng.NextDouble() * Mathf.PI * 2f;

            wordButton.onClick.AddListener(() =>
            {
                ToggleWordSelection(word, wordButton, sub.GetComponent<TextMeshProUGUI>());
            });
        }

        confirmSelectionButton = CreateActionButton(wordsPanel, "ConfirmWords", "Continue", new Vector2(0.5f, 0.08f));
        confirmSelectionButton.interactable = false;
        confirmSelectionButton.onClick.AddListener(ShowRatingsUI);
    }

    private void ToggleWordSelection(string word, Button button, TextMeshProUGUI counterText)
    {
        bool alreadySelected = selectedWords.Contains(word);

        if (alreadySelected)
        {
            selectedWords.Remove(word);
        }
        else
        {
            if (selectedWords.Count >= requiredSelections)
                return;

            selectedWords.Add(word);
        }

        Image image = button.GetComponent<Image>();
        bool nowSelected = selectedWords.Contains(word);
        image.color = nowSelected ? new Color(0.36f, 0.69f, 0.53f, 0.95f) : new Color(0.1f, 0.29f, 0.33f, 0.9f);

        counterText.text = $"Selected: {selectedWords.Count} / {requiredSelections}";
        confirmSelectionButton.interactable = selectedWords.Count == requiredSelections;
    }

    private void ShowRatingsUI()
    {
        wordsPanel.gameObject.SetActive(false);

        ratingPanel = CreatePanel("RatingsPanel", rootCanvas.transform as RectTransform, new Color(0f, 0f, 0f, 0f));
        StretchRect(ratingPanel);

        RectTransform heading = CreateText(
            ratingPanel,
            "RatingsHeading",
            "Rate each word from 1 to 10",
            34,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.93f, 0.95f, 0.89f));

        heading.anchorMin = new Vector2(0.1f, 0.85f);
        heading.anchorMax = new Vector2(0.9f, 0.95f);
        heading.offsetMin = Vector2.zero;
        heading.offsetMax = Vector2.zero;

        for (int i = 0; i < selectedWords.Count; i++)
        {
            string word = selectedWords[i];
            float y = 0.68f - (i * 0.2f);

            RectTransform row = CreatePanel($"Row_{word}", ratingPanel, new Color(0.08f, 0.2f, 0.23f, 0.8f));
            row.anchorMin = new Vector2(0.18f, y - 0.08f);
            row.anchorMax = new Vector2(0.82f, y + 0.08f);
            row.offsetMin = Vector2.zero;
            row.offsetMax = Vector2.zero;

            RectTransform wordText = CreateText(
                row,
                $"Word_{word}",
                word,
                28,
                FontStyles.Bold,
                TextAlignmentOptions.Left,
                new Color(0.92f, 0.95f, 0.91f));

            wordText.anchorMin = new Vector2(0.04f, 0.1f);
            wordText.anchorMax = new Vector2(0.34f, 0.9f);
            wordText.offsetMin = Vector2.zero;
            wordText.offsetMax = Vector2.zero;

            Slider slider = CreateSlider(row, $"Slider_{word}");
            RectTransform sliderRt = slider.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.38f, 0.25f);
            sliderRt.anchorMax = new Vector2(0.86f, 0.75f);
            sliderRt.offsetMin = Vector2.zero;
            sliderRt.offsetMax = Vector2.zero;

            slider.minValue = minSliderValue;
            slider.maxValue = maxSliderValue;
            slider.wholeNumbers = true;
            slider.value = 5;
            ratings[word] = (int)slider.value;

            RectTransform valueText = CreateText(
                row,
                $"Value_{word}",
                $"{slider.value:0}",
                24,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Color(0.99f, 0.95f, 0.71f));

            valueText.anchorMin = new Vector2(0.88f, 0.15f);
            valueText.anchorMax = new Vector2(0.98f, 0.85f);
            valueText.offsetMin = Vector2.zero;
            valueText.offsetMax = Vector2.zero;

            TextMeshProUGUI valueTmp = valueText.GetComponent<TextMeshProUGUI>();
            slider.onValueChanged.AddListener(v =>
            {
                int value = Mathf.RoundToInt(v);
                ratings[word] = value;
                valueTmp.text = value.ToString();
            });
        }

        submitRatingsButton = CreateActionButton(ratingPanel, "SubmitRatings", "Submit", new Vector2(0.5f, 0.1f));
        submitRatingsButton.onClick.AddListener(FinishMiniGame);
    }

    private void FinishMiniGame()
    {
        lastSelectedWords = new List<string>(selectedWords);
        lastRatings = new Dictionary<string, int>(ratings);

        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
        {
            levelManager.SetScene($"\"{CompletionSceneName}\"");
            return;
        }

        Debug.LogError($"No LevelManager found. Cannot transition to scene '{CompletionSceneName}'.");
    }

    private static RectTransform CreatePanel(string name, RectTransform parent, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        Image image = panel.GetComponent<Image>();
        image.color = bgColor;

        return panel.GetComponent<RectTransform>();
    }

    private static Button CreateWordButton(RectTransform parent, string word)
    {
        GameObject go = new GameObject(word + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.1f, 0.29f, 0.33f, 0.9f);

        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.16f, 0.43f, 0.46f, 1f);
        colors.pressedColor = new Color(0.05f, 0.24f, 0.26f, 1f);
        colors.selectedColor = new Color(0.36f, 0.69f, 0.53f, 0.95f);
        button.colors = colors;

        RectTransform textRt = CreateText(go.GetComponent<RectTransform>(), word + "Text", word, 28, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.94f, 0.96f, 0.9f));
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 8f);
        textRt.offsetMax = new Vector2(-10f, -8f);

        return button;
    }

    private static Button CreateActionButton(RectTransform parent, string name, string text, Vector2 anchor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(240f, 72f);
        rt.anchoredPosition = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.78f, 0.55f, 0.28f, 0.95f);

        Button button = go.GetComponent<Button>();

        RectTransform textRt = CreateText(go.GetComponent<RectTransform>(), name + "Text", text, 28, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.1f, 0.1f, 0.1f, 1f));
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return button;
    }

    private static Slider CreateSlider(RectTransform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        Slider slider = go.GetComponent<Slider>();

        RectTransform sliderRt = go.GetComponent<RectTransform>();
        sliderRt.sizeDelta = new Vector2(400f, 24f);

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(go.transform, false);
        Image bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRt = background.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(10f, 0f);
        fillAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.94f, 0.72f, 0.34f, 0.95f);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10f, 0f);
        handleAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.GetComponent<Image>();
        handleImage.color = new Color(0.9f, 0.95f, 0.92f, 1f);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, 36f);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static RectTransform CreateText(RectTransform parent, string name, string content, int size, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        return go.GetComponent<RectTransform>();
    }

    private static void StretchRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
