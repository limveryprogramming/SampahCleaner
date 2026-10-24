using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MainMenuBuilder
{
    const string ScenePath = "Assets/Scenes/MainMenuScene.unity";
    const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    const string SpriteDir = "Assets/UI/Sprites";
    const string RobotPrefabPath = "Assets/Models/LandRobot/LandRobot.prefab";

    static Color Hex(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        c.a = alpha;
        return c;
    }

    [MenuItem("Sampah Cleaner/Build Main Menu UI")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), SpriteDir));
        var fillSprite = MakeRoundedSprite(SpriteDir + "/RoundedFill.png", false);
        var ringSprite = MakeRoundedSprite(SpriteDir + "/RoundedRing.png", true);
        var robotSprite = MakeRobotPreview(SpriteDir + "/RobotPreview.png");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var old = GameObject.Find("MainMenuCanvas");
        if (old != null) Object.DestroyImmediate(old);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGo = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var controller = canvasGo.AddComponent<MainMenuController>();

        var bg = NewUI("Background", canvasGo.transform);
        Stretch(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = Hex("0B1F2E");
        bgImg.raycastTarget = false;

        BuildTitle(canvasGo.transform, font);
        BuildButtonPanel(canvasGo.transform, font, fillSprite, ringSprite, controller);
        BuildRobotCard(canvasGo.transform, fillSprite, ringSprite, robotSprite);

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = canvasGo;
        Debug.Log("Main menu UI built and saved in " + ScenePath);
    }

    static void BuildTitle(Transform parent, Font font)
    {
        var title = NewUI("Title", parent);
        Anchor(title, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, -80), new Vector2(1000, 450));

        AddText(title, "Sampah", "SAMPAH", font, 150, FontStyle.Bold, Hex("39D9D2"), new Vector2(0, 0), new Vector2(1000, 170));
        AddText(title, "Cleaner", "CLEANER", font, 150, FontStyle.Bold, Hex("F7922B"), new Vector2(0, -160), new Vector2(1000, 170));
        AddText(title, "Tagline", "CLEAN THE COASTS, SAVE THE TIDE", font, 36, FontStyle.Normal, Hex("DDF3F6"), new Vector2(0, -345), new Vector2(1000, 50));
        AddText(title, "Version", "v1.0.0", font, 28, FontStyle.Normal, Hex("6F8A99"), new Vector2(0, -395), new Vector2(1000, 40));
    }

    static void BuildButtonPanel(Transform parent, Font font, Sprite fillSprite, Sprite ringSprite, MainMenuController controller)
    {
        var panel = NewUI("ButtonPanel", parent);
        Anchor(panel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-70, 0), new Vector2(540, 760));
        AddSliced(panel, "Fill", fillSprite, Hex("0F2A3D", 0.85f), false);
        AddSliced(panel, "Outline", ringSprite, Hex("2FD3E0", 0.6f), false);

        var list = NewUI("Buttons", panel);
        Stretch(list);
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 50, 50);
        layout.spacing = 26;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var play = MakeButton(list, "PLAY", true, 124, font, fillSprite, ringSprite);
        var unlimited = MakeButton(list, "UNLIMITED MODE", false, 104, font, fillSprite, ringSprite);
        var upgrades = MakeButton(list, "UPGRADES", false, 104, font, fillSprite, ringSprite);
        var settings = MakeButton(list, "SETTINGS", false, 104, font, fillSprite, ringSprite);
        var quit = MakeButton(list, "QUIT", false, 104, font, fillSprite, ringSprite);

        UnityEventTools.AddPersistentListener(play.onClick, controller.Play);
        UnityEventTools.AddPersistentListener(unlimited.onClick, controller.UnlimitedMode);
        UnityEventTools.AddPersistentListener(upgrades.onClick, controller.Upgrades);
        UnityEventTools.AddPersistentListener(settings.onClick, controller.Settings);
        UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
    }

    static void BuildRobotCard(Transform parent, Sprite fillSprite, Sprite ringSprite, Sprite robotSprite)
    {
        var card = NewUI("RobotCard", parent);
        Anchor(card, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(80, 80), new Vector2(360, 360));
        AddSliced(card, "Fill", fillSprite, Hex("0F2A3D", 0.85f), false);
        AddSliced(card, "Outline", ringSprite, Hex("2FD3E0", 0.6f), false);

        if (robotSprite == null) return;
        var robot = NewUI("Robot", card);
        Stretch(robot, 24);
        var img = robot.gameObject.AddComponent<Image>();
        img.sprite = robotSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    static Button MakeButton(Transform parent, string text, bool primary, float height, Font font, Sprite fillSprite, Sprite ringSprite)
    {
        var rt = NewUI("Btn_" + text, parent);
        var fill = rt.gameObject.AddComponent<Image>();
        fill.sprite = fillSprite;
        fill.type = Image.Type.Sliced;

        var button = rt.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = fill;

        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        var outline = AddSliced(rt, "Outline", ringSprite, Color.white, false);

        var label = AddText(rt, "Label", text, font, primary ? 48 : 36, FontStyle.Bold, Color.white, Vector2.zero, Vector2.zero);
        Stretch(label.rectTransform);
        label.alignment = TextAnchor.MiddleCenter;

        var fx = rt.gameObject.AddComponent<MenuButtonFX>();
        fx.fill = fill;
        fx.outline = outline;
        fx.label = label;

        if (primary)
        {
            fx.normalFill = Hex("F7821B"); fx.hoverFill = Hex("FFB04D"); fx.pressedFill = Hex("D9680A");
            fx.normalOutline = Hex("38E1E8"); fx.hoverOutline = Hex("FFFFFF");
            fx.normalText = Hex("3B1D00"); fx.hoverText = Hex("2A1300");
        }
        else
        {
            fx.normalFill = Hex("12344A", 0.9f); fx.hoverFill = Hex("1EB5C4"); fx.pressedFill = Hex("148A97");
            fx.normalOutline = Hex("2FD3E0", 0.55f); fx.hoverOutline = Hex("A8FBFF");
            fx.normalText = Hex("D6F3F7"); fx.hoverText = Hex("06202E");
        }

        fill.color = fx.normalFill;
        outline.color = fx.normalOutline;
        label.color = fx.normalText;
        return button;
    }

    static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Image AddSliced(RectTransform parent, string name, Sprite sprite, Color color, bool raycast)
    {
        var rt = NewUI(name, parent);
        Stretch(rt);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    static Text AddText(Transform parent, string name, string content, Font font, int size, FontStyle style, Color color, Vector2 pos, Vector2 boxSize)
    {
        var rt = NewUI(name, parent);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = boxSize;
        var t = rt.gameObject.AddComponent<Text>();
        t.text = content;
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.UpperLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Sprite MakeRoundedSprite(string assetPath, bool ring)
    {
        const int size = 128;
        const float radius = 40f;
        const float ringWidth = 4f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float qx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float qy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                float outer = Mathf.Clamp01(0.5f - d);
                float a = outer;
                if (ring) a = outer * (1f - Mathf.Clamp01(0.5f - (d + ringWidth)));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), assetPath), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(42, 42, 42, 42);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    static Sprite MakeRobotPreview(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RobotPrefabPath);
        if (prefab == null) return null;

        GameObject instance = null, camGo = null;
        RenderTexture rt = null;
        try
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = new Vector3(2000, 0, 0);
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return null;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            camGo = new GameObject("PreviewCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            var dir = new Vector3(0.6f, 0.35f, 0.8f).normalized;
            camGo.transform.position = bounds.center + dir * bounds.extents.magnitude * 4.3f;
            camGo.transform.LookAt(bounds.center);

            rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), assetPath), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Robot preview skipped: " + e.Message);
            return null;
        }
        finally
        {
            if (camGo != null) Object.DestroyImmediate(camGo);
            if (instance != null) Object.DestroyImmediate(instance);
            if (rt != null) { rt.Release(); Object.DestroyImmediate(rt); }
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
