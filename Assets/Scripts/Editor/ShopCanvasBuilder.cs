using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class ShopCanvasBuilder
{
    const int RowCount = 4;

    [MenuItem("Tools/Build Shop Canvas")]
    static void Build()
    {
        var existing = Object.FindFirstObjectByType<ShopUI>();
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            Debug.LogWarning("ShopCanvas already exists in this scene. Delete it first to rebuild.");
            return;
        }

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGo = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Build Shop Canvas");
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        var panel = Image("ShopPanel", canvasGo.transform, new Color(0.08f, 0.1f, 0.14f, 0.95f));
        Place(panel, new Vector2(0.5f, 0.5f), new Vector2(620, 490), Vector2.zero);

        var title = Label("Title", panel.transform, font, "SHOP", 38, TextAnchor.MiddleCenter, Color.white);
        Place(title.gameObject, new Vector2(0.5f, 1f), new Vector2(580, 50), new Vector2(0, -35));

        var coins = Label("CoinsText", panel.transform, font, "Coins: 0", 24, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f));
        Place(coins.gameObject, new Vector2(0.5f, 1f), new Vector2(580, 34), new Vector2(0, -78));

        var rows = new ShopUI.Row[RowCount];
        for (int i = 0; i < RowCount; i++)
        {
            var row = Image("UpgradeRow" + (i + 1), panel.transform, new Color(1f, 1f, 1f, 0.08f));
            Place(row, new Vector2(0.5f, 1f), new Vector2(580, 70), new Vector2(0, -135 - i * 80));

            var name = Label("Name", row.transform, font, "Upgrade", 24, TextAnchor.MiddleLeft, Color.white);
            Place(name.gameObject, new Vector2(0f, 1f), new Vector2(380, 34), new Vector2(205, -20));

            var desc = Label("Description", row.transform, font, "Description", 18, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.8f));
            Place(desc.gameObject, new Vector2(0f, 0f), new Vector2(380, 30), new Vector2(205, 20));

            var buy = ButtonObj("BuyButton", row.transform, font, "Buy", new Color(0.2f, 0.55f, 0.3f), out var buyLabel);
            Place(buy.gameObject, new Vector2(1f, 0.5f), new Vector2(150, 48), new Vector2(-90, 0));

            rows[i] = new ShopUI.Row { title = name, description = desc, buyButton = buy, buyLabel = buyLabel };
        }

        var close = ButtonObj("CloseButton", panel.transform, font, "Close (R)", new Color(0.5f, 0.2f, 0.2f), out _);
        Place(close.gameObject, new Vector2(0.5f, 0f), new Vector2(200, 44), new Vector2(0, 34));

        var shop = canvasGo.AddComponent<ShopUI>();
        shop.panel = panel;
        shop.coinsText = coins;
        shop.closeButton = close;
        shop.rows = rows;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Build Shop Canvas");
        }

        Selection.activeObject = canvasGo;
        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        Debug.Log("ShopCanvas created. Save the scene (Ctrl+S).");
    }

    static GameObject Image(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<UnityEngine.UI.Image>().color = color;
        return go;
    }

    static Text Label(string name, Transform parent, Font font, string text, int size, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static Button ButtonObj(string name, Transform parent, Font font, string text, Color color, out Text label)
    {
        var go = Image(name, parent, color);
        var b = go.AddComponent<Button>();
        var colors = b.colors;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.9f);
        b.colors = colors;
        label = Label("Label", go.transform, font, text, 22, TextAnchor.MiddleCenter, Color.white);
        var rt = label.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return b;
    }

    static void Place(GameObject go, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }
}
