using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// Buduje cały interfejs VR w kodzie: HUD (HP zamka, czas, bloki, przycisk pauzy)
/// oraz menu (suwaki przeciwników i meteorytów, Start/Wznów, Reset).
/// Powieś na pustym obiekcie "GameUI". Nic więcej nie trzeba tworzyć ręcznie.
public class GameUI : MonoBehaviour
{
    public Transform xrOrigin;                    // puste = root kamery
    public Camera playerCamera;                   // puste = Camera.main
    public CastleHealth castle;
    public float distance = 2f;                   // odległość panelu od głowy (mnożona przez skalę gigiganta)
    public InputActionReference toggleMenuAction; // opcjonalnie: przycisk Menu na kontrolerze

    Font font;
    Canvas canvas;
    Text hpText, timerText, blockText, titleText, enemyLabel, meteorLabel, primaryLabel;
    RectTransform hpFill;
    Image hpFillImage;
    GameObject menuPanel, pauseButton, primaryButton;
    GameState lastState = (GameState)(-1);

    static readonly Color PanelColor = new Color(0.05f, 0.07f, 0.12f, 0.88f);
    static readonly Color ButtonColor = new Color(0.2f, 0.35f, 0.6f, 1f);

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (xrOrigin == null && playerCamera != null) xrOrigin = playerCamera.transform.root;
        if (castle == null) castle = FindFirstObjectByType<CastleHealth>();

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<XRUIInputModule>();
        }

        BuildUI();
        PlaceInFront();

        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggle;
        }
    }

    void OnDestroy()
    {
        if (toggleMenuAction != null) toggleMenuAction.action.performed -= OnToggle;
    }

    void OnToggle(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || canvas == null) return;

        if (gm.State != lastState)
        {
            lastState = gm.State;
            RefreshState(gm.State);
        }

        if (castle != null)
        {
            float ratio = castle.maxHealth > 0f ? castle.Current / castle.maxHealth : 0f;
            hpText.text = "ZAMEK HP: " + Mathf.CeilToInt(castle.Current) + " / " + Mathf.CeilToInt(castle.maxHealth);
            hpFill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
            hpFillImage.color = Color.Lerp(new Color(0.85f, 0.15f, 0.1f), new Color(0.2f, 0.8f, 0.25f), ratio);
        }

        int t = Mathf.CeilToInt(gm.TimeLeft);
        timerText.text = string.Format("Czas: {0}:{1:00}", t / 60, t % 60);
        blockText.text = "Bloki: " + gm.BlockedCount;
        enemyLabel.text = "Przeciwnicy (katapulty/armaty): " + GameManager.EnemyCount;
        meteorLabel.text = "Meteoryty na minutę: " + GameManager.MeteorsPerMinute;
    }

    void RefreshState(GameState state)
    {
        bool showMenu = state != GameState.Playing;
        menuPanel.SetActive(showMenu);
        pauseButton.SetActive(state == GameState.Playing);

        switch (state)
        {
            case GameState.Ready: titleText.text = "OBRONA ZAMKU"; break;
            case GameState.Paused: titleText.text = "PAUZA"; break;
            case GameState.Won: titleText.text = "ZWYCIĘSTWO!"; break;
            case GameState.Lost: titleText.text = "GAME OVER"; break;
        }

        bool canStart = state == GameState.Ready || state == GameState.Paused;
        primaryButton.SetActive(canStart);
        primaryLabel.text = state == GameState.Ready ? "START" : "WZNÓW";

        if (showMenu) PlaceInFront();
    }

    void PlaceInFront()
    {
        if (playerCamera == null) return;
        float s = xrOrigin != null ? xrOrigin.lossyScale.y : 1f;

        Vector3 fwd = playerCamera.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 pos = playerCamera.transform.position + fwd * distance * s + Vector3.down * 0.15f * s;
        canvas.transform.position = pos;
        canvas.transform.rotation = Quaternion.LookRotation(pos - playerCamera.transform.position);
        canvas.transform.localScale = Vector3.one * 0.0015f * s;
    }

    // ---------------------------------------------------------------- budowanie UI

    void BuildUI()
    {
        var go = new GameObject("VR Game Canvas", typeof(RectTransform));
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        go.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 520f);

        // --- HUD (zawsze widoczny)
        hpText = MakeText(rt, "", 30, V(0f, 0.9f), V(0.42f, 1f), TextAnchor.MiddleLeft);
        timerText = MakeText(rt, "", 30, V(0.42f, 0.9f), V(0.66f, 1f), TextAnchor.MiddleCenter);
        blockText = MakeText(rt, "", 26, V(0.66f, 0.9f), V(0.86f, 1f), TextAnchor.MiddleCenter);

        var barBg = MakeRect("HPBarBG", rt, V(0f, 0.84f), V(1f, 0.89f));
        barBg.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        hpFill = MakeRect("HPBarFill", barBg, V(0f, 0f), V(1f, 1f));
        hpFillImage = hpFill.gameObject.AddComponent<Image>();

        var pause = MakeButton(rt, "II", V(0.87f, 0.9f), V(1f, 1f), () => GameManager.Instance.Pause(), out _);
        pauseButton = pause.gameObject;

        // --- Menu
        var panel = MakeRect("Menu", rt, V(0f, 0f), V(1f, 0.8f));
        panel.gameObject.AddComponent<Image>().color = PanelColor;
        menuPanel = panel.gameObject;

        titleText = MakeText(panel, "", 44, V(0f, 0.84f), V(1f, 1f), TextAnchor.MiddleCenter);

        enemyLabel = MakeText(panel, "", 28, V(0.05f, 0.70f), V(0.95f, 0.82f), TextAnchor.MiddleLeft);
        MakeSlider(panel, V(0.05f, 0.58f), V(0.95f, 0.70f), 0, 10, GameManager.EnemyCount,
                   v => GameManager.EnemyCount = (int)v);

        meteorLabel = MakeText(panel, "", 28, V(0.05f, 0.44f), V(0.95f, 0.56f), TextAnchor.MiddleLeft);
        MakeSlider(panel, V(0.05f, 0.32f), V(0.95f, 0.44f), 0, 60, GameManager.MeteorsPerMinute,
                   v => GameManager.MeteorsPerMinute = (int)v);

        var primary = MakeButton(panel, "START", V(0.05f, 0.04f), V(0.48f, 0.26f),
                                 () => GameManager.Instance.StartOrResume(), out primaryLabel);
        primaryButton = primary.gameObject;

        MakeButton(panel, "RESET", V(0.52f, 0.04f), V(0.95f, 0.26f),
                   () => GameManager.Instance.ResetGame(), out _);
    }

    static Vector2 V(float x, float y) { return new Vector2(x, y); }

    RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    Text MakeText(Transform parent, string content, int size, Vector2 aMin, Vector2 aMax, TextAnchor anchor)
    {
        var rt = MakeRect("Text", parent, aMin, aMax);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button MakeButton(Transform parent, string label, Vector2 aMin, Vector2 aMax, UnityAction onClick, out Text labelText)
{
    var rt = MakeRect("Button " + label, parent, aMin, aMax);
    var img = rt.gameObject.AddComponent<Image>();
    img.color = ButtonColor;
    var btn = rt.gameObject.AddComponent<Button>();
    btn.targetGraphic = img;

    var c = btn.colors;
    c.normalColor = ButtonColor;
    c.highlightedColor = new Color(0.4f, 0.65f, 1f);
    c.pressedColor = new Color(0.1f, 0.2f, 0.4f);
    c.selectedColor = ButtonColor;
    btn.colors = c;

    btn.onClick.AddListener(onClick);
    labelText = MakeText(rt, label, 32, V(0f, 0f), V(1f, 1f), TextAnchor.MiddleCenter);
    return btn;
}

    Slider MakeSlider(Transform parent, Vector2 aMin, Vector2 aMax, int min, int max, int value, UnityAction<float> onChange)
    {
        var root = MakeRect("Slider", parent, aMin, aMax);

        var bg = MakeRect("Background", root, V(0f, 0.3f), V(1f, 0.7f));
        bg.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

        var fillArea = MakeRect("Fill Area", root, V(0f, 0.3f), V(1f, 0.7f));
        fillArea.offsetMin = new Vector2(5f, 0f);
        fillArea.offsetMax = new Vector2(-15f, 0f);
        var fill = MakeRect("Fill", fillArea, V(0f, 0f), V(1f, 1f));
        fill.gameObject.AddComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f, 1f);

        var handleArea = MakeRect("Handle Slide Area", root, V(0f, 0f), V(1f, 1f));
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);
        var handle = MakeRect("Handle", handleArea, V(0f, 0f), V(0f, 1f));
        handle.sizeDelta = new Vector2(30f, 0f);
        var handleImg = handle.gameObject.AddComponent<Image>();
        handleImg.color = Color.white;

        var s = root.gameObject.AddComponent<Slider>();
        s.fillRect = fill;
        s.handleRect = handle;
        s.targetGraphic = handleImg;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = min;
        s.maxValue = max;
        s.wholeNumbers = true;
        s.value = value;
        s.onValueChanged.AddListener(onChange);
        return s;
    }
}
