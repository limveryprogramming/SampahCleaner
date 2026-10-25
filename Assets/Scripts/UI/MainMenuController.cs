using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] string map1Scene = "Map1";
    [SerializeField] string map2Scene = "Map2";

    GameObject mapPanel;

    public void Play()
    {
        if (mapPanel == null) mapPanel = BuildMapPanel();
        mapPanel.SetActive(true);
    }

    public void UnlimitedMode() => Debug.Log("Unlimited Mode: not built yet.");

    public void Upgrades() => Debug.Log("Upgrades: not built yet.");

    public void Settings() => Debug.Log("Settings: not built yet.");

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (mapPanel != null && mapPanel.activeSelf && UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            mapPanel.SetActive(false);
    }

    GameObject BuildMapPanel()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var panel = NewUI("MapSelectPanel", canvas.transform);
        Stretch(panel);
        panel.AddComponent<Image>().color = new Color(0.02f, 0.07f, 0.12f, 0.92f);

        var title = NewUI("Title", panel.transform);
        var tr = title.GetComponent<RectTransform>();
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.sizeDelta = new Vector2(900, 90);
        tr.anchoredPosition = new Vector2(0, 230);
        MakeText(title, font, "CHOOSE A MAP", 64, FontStyle.Bold, new Color(0.18f, 0.83f, 0.88f));

        MakeMapButton(panel.transform, font, "BEACH TOWN", "Map 1", new Vector2(-270, 20), () => Load(map1Scene));
        MakeMapButton(panel.transform, font, "OCEAN REEF", "Map 2", new Vector2(270, 20), () => Load(map2Scene));

        var back = MakeButton(panel.transform, font, "BACK", new Vector2(0, -230), new Vector2(260, 70), 30,
            new Color(0.07f, 0.2f, 0.29f, 0.9f), new Color(0.12f, 0.71f, 0.77f), new Color(0.08f, 0.54f, 0.59f),
            new Color(0.84f, 0.95f, 0.97f), new Color(0.02f, 0.13f, 0.18f));
        back.onClick.AddListener(() => panel.SetActive(false));

        return panel;
    }

    void MakeMapButton(Transform parent, Font font, string name, string sub, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var btn = MakeButton(parent, font, name, pos, new Vector2(460, 300), 44,
            new Color(0.07f, 0.2f, 0.29f, 0.9f), new Color(0.12f, 0.71f, 0.77f), new Color(0.08f, 0.54f, 0.59f),
            new Color(0.84f, 0.95f, 0.97f), new Color(0.02f, 0.13f, 0.18f));
        btn.onClick.AddListener(onClick);

        var subGo = NewUI("Sub", btn.transform);
        var sr = subGo.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0, 0); sr.anchorMax = new Vector2(1, 0);
        sr.pivot = new Vector2(0.5f, 0); sr.sizeDelta = new Vector2(0, 70); sr.anchoredPosition = new Vector2(0, 20);
        var st = MakeText(subGo, font, sub, 28, FontStyle.Normal, new Color(0.84f, 0.95f, 0.97f));
        st.raycastTarget = false;
    }

    Button MakeButton(Transform parent, Font font, string text, Vector2 pos, Vector2 size, int fontSize,
        Color fillN, Color fillH, Color fillP, Color textN, Color textH)
    {
        var go = NewUI("Btn_" + text, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = pos;

        var outline = go.AddComponent<Image>();
        outline.color = new Color(0.18f, 0.83f, 0.88f, 0.55f);

        var fillGo = NewUI("Fill", go.transform);
        Stretch(fillGo);
        fillGo.GetComponent<RectTransform>().offsetMin = new Vector2(3, 3);
        fillGo.GetComponent<RectTransform>().offsetMax = new Vector2(-3, -3);
        var fill = fillGo.AddComponent<Image>();
        fill.color = fillN;
        fill.raycastTarget = false;

        var labelGo = NewUI("Label", go.transform);
        Stretch(labelGo);
        var label = MakeText(labelGo, font, text, fontSize, FontStyle.Bold, textN);
        label.raycastTarget = false;

        var button = go.AddComponent<Button>();
        button.targetGraphic = outline;
        button.transition = Selectable.Transition.None;

        var fx = go.AddComponent<MenuButtonFX>();
        fx.fill = fill; fx.outline = outline; fx.label = label;
        fx.normalFill = fillN; fx.hoverFill = fillH; fx.pressedFill = fillP;
        fx.normalOutline = new Color(0.18f, 0.83f, 0.88f, 0.55f); fx.hoverOutline = new Color(0.66f, 0.98f, 1f);
        fx.normalText = textN; fx.hoverText = textH;
        return button;
    }

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Text MakeText(GameObject go, Font font, string text, int size, FontStyle style, Color color)
    {
        var t = go.AddComponent<Text>();
        t.font = font; t.text = text; t.fontSize = size; t.fontStyle = style; t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    static void Load(string scene) => SceneManager.LoadScene(scene);
}
