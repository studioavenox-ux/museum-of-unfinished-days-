using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>Builds and presents the compact in-game inspection, journal, and pause interface.</summary>
public sealed class GalleryUI : MonoBehaviour
{
    public enum Screen { Gameplay, Inspection, Journal, Clockwork, StoryDiscovery, StoryDialogue, StoryChoice, StoryOutcome, Pause }
    private const string Gold = "#C69A55";
    private const string Paper = "#E8DFC9";
    private const string Muted = "#B6B7AE";
    private Canvas canvas;
    private Font font;
    private GameObject overlay;
    private Text titleText, bodyText, promptText, reticleText, journalHeading, journalHint;
    private RectTransform listContent;
    private ScrollRect evidenceScroll;
    private Button shareChoiceButton, keepChoiceButton, storyReturnButton;
    private Screen activeScreen;
    private EvidenceJournal journal;

    public Screen ActiveScreen => activeScreen;
    public bool IsBlocking => activeScreen != Screen.Gameplay;

    public void Initialize(EvidenceJournal evidence)
    {
        journal = evidence;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        new GameObject("UI Event System", typeof(EventSystem), typeof(StandaloneInputModule));
        BuildCanvas();
        Show(Screen.Gameplay);
    }

    public void SetTarget(GalleryClue clue, ClockworkPuzzle puzzle)
    {
        if (activeScreen != Screen.Gameplay) return;
        promptText.text = puzzle != null ? "[ E ]   ALIGN THE CLOCKWORK" :
            clue != null ? "[ E ]   INSPECT   " + clue.title.ToUpperInvariant() : "";
    }

    public void OpenInspection(GalleryClue clue)
    {
        titleText.text = clue.title;
        bodyText.text = clue.description + "\n\nYour inspection has been added to the evidence journal.";
        Show(Screen.Inspection);
    }

    public void OpenClockwork(string status)
    {
        titleText.text = "THE CELESTIAL REGULATOR";
        bodyText.text = status;
        Show(Screen.Clockwork);
    }

    public void UpdateClockwork(string status, bool solved)
    {
        titleText.text = solved ? "THE RECORD REMEMBERS" : "THE CELESTIAL REGULATOR";
        bodyText.text = status;
        titleText.color = Hex(Gold);
    }

    public void ShowStoryDiscovery(string title, string text)
    {
        titleText.text = title.ToUpperInvariant();
        bodyText.text = text + "\n\n[ ENTER / SPACE ]  Continue to the curator's intercom";
        Show(Screen.StoryDiscovery);
    }

    public void ShowStoryDialogue(string speaker, string line, int index, int total)
    {
        titleText.text = speaker;
        bodyText.text = line + "\n\n[ ENTER / SPACE ]  Continue   ·   " + (index + 1) + " / " + total;
        Show(Screen.StoryDialogue);
    }

    public void ShowStoryChoice(System.Action<int> onChoose)
    {
        titleText.text = "WHAT WILL NOAH TELL HER?";
        bodyText.text = "The intercom waits. Choose a response with 1 / 2 or click below. Escape will not discard this choice.";
        shareChoiceButton.onClick.RemoveAllListeners();
        shareChoiceButton.onClick.AddListener(() => onChoose(0));
        keepChoiceButton.onClick.RemoveAllListeners();
        keepChoiceButton.onClick.AddListener(() => onChoose(1));
        Show(Screen.StoryChoice);
    }

    public void ShowStoryOutcome(string title, string text, System.Action onContinue)
    {
        titleText.text = title;
        bodyText.text = text + "\n\n[ ENTER / ESCAPE ]  Return to the gallery";
        storyReturnButton.onClick.RemoveAllListeners();
        storyReturnButton.onClick.AddListener(() => onContinue());
        Show(Screen.StoryOutcome);
    }

    public void ToggleJournal()
    {
        if (activeScreen == Screen.Journal) Show(Screen.Gameplay);
        else if (activeScreen == Screen.Gameplay) Show(Screen.Journal);
    }

    public void RefreshJournal()
    {
        foreach (Transform child in listContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        if (journal.Entries.Count == 0)
        {
            AddEntry("No evidence collected", "Inspect an unusual artifact in the gallery. Its story will be recorded here.", true);
            return;
        }
        foreach (GalleryClue clue in journal.Entries) AddEntry(clue.title, clue.description, false);
        if (evidenceScroll != null) evidenceScroll.verticalNormalizedPosition = 1f;
    }

    public void Show(Screen screen)
    {
        activeScreen = screen;
        overlay.SetActive(screen != Screen.Gameplay);
        promptText.gameObject.SetActive(screen == Screen.Gameplay);
        reticleText.gameObject.SetActive(screen == Screen.Gameplay);
        if (screen == Screen.Journal) RefreshJournal();
        if (screen == Screen.Pause) { titleText.text = "PAUSED"; bodyText.text = "Your unfinished day waits."; }
        Cursor.lockState = screen == Screen.Gameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = screen != Screen.Gameplay;
        SetPanelMode(screen);
    }

    private void BuildCanvas()
    {
        var root = new GameObject("Gameplay Interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        reticleText = Label(root.transform, "+", 22, Paper, TextAnchor.MiddleCenter);
        Place(reticleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(30, 30), Vector2.zero);
        promptText = Label(root.transform, "", 16, Paper, TextAnchor.MiddleCenter);
        Place(promptText.rectTransform, new Vector2(0.5f, 0.43f), new Vector2(540, 42), Vector2.zero);
        overlay = new GameObject("Modal backdrop", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(root.transform, false);
        var bg = overlay.GetComponent<Image>(); bg.color = new Color(0.012f, 0.035f, 0.045f, 0.91f);
        Stretch(overlay.GetComponent<RectTransform>());
        var panel = new GameObject("Brass edged panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);
        panel.GetComponent<Image>().color = new Color(0.035f, 0.075f, 0.085f, 0.99f);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f); panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760, 540); panelRect.anchoredPosition = Vector2.zero;
        titleText = Label(panel.transform, "", 32, Hex(Gold), TextAnchor.MiddleLeft);
        Place(titleText.rectTransform, new Vector2(0.08f, 0.83f), new Vector2(630, 70), Vector2.zero);
        bodyText = Label(panel.transform, "", 20, Hex(Paper), TextAnchor.UpperLeft);
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        Place(bodyText.rectTransform, new Vector2(0.08f, 0.5f), new Vector2(630, 230), Vector2.zero);
        journalHeading = Label(panel.transform, "EVIDENCE JOURNAL", 30, Hex(Gold), TextAnchor.MiddleLeft);
        Place(journalHeading.rectTransform, new Vector2(0.08f, 0.84f), new Vector2(630, 62), Vector2.zero);
        journalHint = Label(panel.transform, "Scroll to read all collected evidence.", 14, Hex(Muted), TextAnchor.MiddleCenter);
        Place(journalHint.rectTransform, new Vector2(0.5f, 0.18f), new Vector2(550, 28), Vector2.zero);
        var list = new GameObject("Evidence list", typeof(RectTransform), typeof(ScrollRect));
        list.transform.SetParent(panel.transform, false);
        RectTransform listRect = list.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0.08f, 0.24f); listRect.anchorMax = new Vector2(0.92f, 0.75f);
        listRect.offsetMin = Vector2.zero; listRect.offsetMax = Vector2.zero;
        var viewport = new GameObject("Evidence viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(list.transform, false);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        var content = new GameObject("Evidence entries", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        listContent = content.GetComponent<RectTransform>();
        listContent.anchorMin = new Vector2(0, 1); listContent.anchorMax = new Vector2(1, 1);
        listContent.pivot = new Vector2(0.5f, 1); listContent.sizeDelta = Vector2.zero; listContent.anchoredPosition = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>(); layout.spacing = 14; layout.childControlHeight = false; layout.childForceExpandHeight = false;
        var fitter = content.GetComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        evidenceScroll = list.GetComponent<ScrollRect>(); evidenceScroll.viewport = viewport.GetComponent<RectTransform>(); evidenceScroll.content = listContent;
        evidenceScroll.horizontal = false; evidenceScroll.vertical = true; evidenceScroll.movementType = ScrollRect.MovementType.Clamped;
        Button(panel.transform, "CLOSE   [ ESC ]", new Vector2(0.5f, 0.1f), () => ClosePanel());
        Button(panel.transform, "RESUME", new Vector2(0.5f, 0.22f), () => ClosePanel()).gameObject.name = "Resume Button";
        Button(panel.transform, "RETURN TO MAIN MENU (UNAVAILABLE)", new Vector2(0.5f, 0.1f), () => { }).interactable = false;
        shareChoiceButton = ChoiceButton(panel.transform, "1  ·  TELL ELARA WHAT THE RECORD SAYS\nShare the inscription and ask what she knows.", new Vector2(0.5f, 0.31f));
        keepChoiceButton = ChoiceButton(panel.transform, "2  ·  KEEP THE RECORD PRIVATE\nSay you found nothing conclusive.", new Vector2(0.5f, 0.13f));
        storyReturnButton = Button(panel.transform, "RETURN TO GALLERY", new Vector2(0.5f, 0.1f), () => { });
    }

    private void SetPanelMode(Screen screen)
    {
        titleText.transform.parent.Find("Evidence list").gameObject.SetActive(screen == Screen.Journal);
        titleText.gameObject.SetActive(screen != Screen.Journal);
        journalHeading.gameObject.SetActive(screen == Screen.Journal);
        journalHint.gameObject.SetActive(screen == Screen.Journal);
        bool showBody = screen == Screen.Inspection || screen == Screen.Clockwork || screen == Screen.Pause ||
                        screen == Screen.StoryDiscovery || screen == Screen.StoryDialogue || screen == Screen.StoryChoice || screen == Screen.StoryOutcome;
        bodyText.gameObject.SetActive(showBody);
        Transform parent = titleText.transform.parent;
        parent.Find("Button - CLOSE   [ ESC ]").gameObject.SetActive(screen == Screen.Inspection || screen == Screen.Journal || screen == Screen.Clockwork);
        parent.Find("Resume Button").gameObject.SetActive(screen == Screen.Pause);
        parent.Find("Button - RETURN TO MAIN MENU (UNAVAILABLE)").gameObject.SetActive(screen == Screen.Pause);
        shareChoiceButton.gameObject.SetActive(screen == Screen.StoryChoice);
        keepChoiceButton.gameObject.SetActive(screen == Screen.StoryChoice);
        storyReturnButton.gameObject.SetActive(screen == Screen.StoryOutcome);
    }

    private void ClosePanel() { Show(Screen.Gameplay); }

    private void AddEntry(string title, string description, bool empty)
    {
        var entry = new GameObject("Evidence entry", typeof(RectTransform), typeof(VerticalLayoutGroup));
        entry.transform.SetParent(listContent, false);
        var group = entry.GetComponent<VerticalLayoutGroup>(); group.spacing = 3; group.childControlHeight = false; group.childForceExpandHeight = false;
        Text heading = Label(entry.transform, title, 20, empty ? Hex(Muted) : Hex(Gold), TextAnchor.MiddleLeft);
        heading.rectTransform.sizeDelta = new Vector2(600, 30);
        Text detail = Label(entry.transform, description, 16, Hex(Paper), TextAnchor.UpperLeft);
        detail.horizontalOverflow = HorizontalWrapMode.Wrap;
        detail.rectTransform.sizeDelta = new Vector2(600, 76);
        LayoutElement h = heading.gameObject.AddComponent<LayoutElement>(); h.preferredHeight = 30;
        LayoutElement d = detail.gameObject.AddComponent<LayoutElement>(); d.preferredHeight = 76;
    }

    private Button Button(Transform parent, string text, Vector2 anchor, System.Action action)
    {
        var go = new GameObject("Button - " + text, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>(); image.color = new Color(0.11f, 0.15f, 0.15f);
        Button button = go.GetComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = button.colors; colors.highlightedColor = new Color(0.36f, 0.25f, 0.11f); button.colors = colors;
        button.onClick.AddListener(() => action());
        Text label = Label(go.transform, text, 17, Hex(Paper), TextAnchor.MiddleCenter);
        Stretch(label.rectTransform);
        var rect = go.GetComponent<RectTransform>(); rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(300, 46); rect.anchoredPosition = Vector2.zero;
        return button;
    }

    private Button ChoiceButton(Transform parent, string text, Vector2 anchor)
    {
        var go = new GameObject("Story choice - " + text.Substring(0, 1), typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>(); image.color = new Color(0.10f, 0.14f, 0.15f);
        Button button = go.GetComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = button.colors; colors.highlightedColor = new Color(0.34f, 0.24f, 0.12f); button.colors = colors;
        Text label = Label(go.transform, text, 16, Hex(Paper), TextAnchor.MiddleLeft);
        Stretch(label.rectTransform);
        label.rectTransform.offsetMin = new Vector2(18, 4);
        label.rectTransform.offsetMax = new Vector2(-12, -4);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(630, 76); rect.anchoredPosition = Vector2.zero;
        return button;
    }

    private Text Label(Transform parent, string value, int size, Color color, TextAnchor alignment)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment; text.supportRichText = false;
        return text;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 offset)
    { rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = size; rect.anchoredPosition = offset; }
    private static void Stretch(RectTransform rect)
    { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static Color Hex(string html) { ColorUtility.TryParseHtmlString(html, out Color color); return color; }
}
