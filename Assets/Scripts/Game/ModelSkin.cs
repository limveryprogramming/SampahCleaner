using System.Collections.Generic;
using UnityEngine;

// Dresses a placeholder object with a random textured FBX from Resources/<folder>.
public static class ModelSkin
{
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
    static readonly Dictionary<string, float> XOverride = new Dictionary<string, float>
    {
        { "Meshy_AI_fishing_trawler_class_1025034021_texture", 270f },
        { "Meshy_AI_fishing_boat_modern_1025034007_texture", 270f },
    };
    static readonly Dictionary<string, string> SingleModel = new Dictionary<string, string>
    {
        { "Ships", "Meshy_AI_cargo_ship_container__1025033823_texture" },
        { "FishingBoats", "Meshy_AI_fishing_boat_modern_1025034007_texture" },
        { "Characters", "Meshy_AI_beach_character_man_h_1024143600_texture" },
    };
    static readonly Dictionary<string, GameObject[]> cache = new Dictionary<string, GameObject[]>();

    // Returns the attached model, or null if the folder has none. Model is scaled so its height (or longest side
    // if keepLongest) equals targetSize, centred on x/z, with its lowest point at localBottomY.
    public static GameObject Attach(Transform parent, string folder, float targetSize, float localBottomY, bool standUp, Vector3 eulerOffset = default, Vector3 modelEuler = default)
    {
        if (!cache.TryGetValue(folder, out var prefabs)) cache[folder] = prefabs = Resources.LoadAll<GameObject>(folder);
        if (prefabs.Length == 0) return null;

        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        if (SingleModel.TryGetValue(folder, out var only))
            foreach (var p in prefabs) if (p.name == only) { prefab = p; break; }
        if (eulerOffset.x != 0f && XOverride.TryGetValue(prefab.name, out float ox)) eulerOffset.x = ox;
        bool fixedRotation = eulerOffset != Vector3.zero;
        bool multiAxis = (eulerOffset.x != 0f ? 1 : 0) + (eulerOffset.y != 0f ? 1 : 0) + (eulerOffset.z != 0f ? 1 : 0) > 1;

        // Multi-axis rotations get one pivot per axis (Y, then X, then Z, matching Unity's order) so each
        // object shows a single clean angle instead of an equivalent but confusing Euler triple.
        Transform holder = parent;
        if (multiAxis)
        {
            holder = Pivot("Rot_Y", holder, new Vector3(0f, eulerOffset.y, 0f));
            holder = Pivot("Rot_X", holder, new Vector3(eulerOffset.x, 0f, 0f));
            holder = Pivot("Rot_Z", holder, new Vector3(0f, 0f, eulerOffset.z));
        }

        var go = Object.Instantiate(prefab, holder);
        go.name = prefab.name;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = multiAxis ? modelEuler : eulerOffset;
        go.transform.localScale = Vector3.one;

        var mat = GetMaterial(folder, prefab.name);
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) { r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
        if (renderers.Length == 0) return go;

        Bounds b = Bounds(renderers);
        if (standUp && !fixedRotation && b.size.y < Mathf.Min(b.size.x, b.size.z) * 0.9f)
        {
            go.transform.localRotation = b.size.z >= b.size.x ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.Euler(0f, 0f, 90f);
            b = Bounds(renderers);
        }

        float size = standUp ? b.size.y : Mathf.Max(b.size.x, b.size.y, b.size.z);
        float s = targetSize / size;
        go.transform.localScale = Vector3.one * s;
        b = Bounds(renderers);

        Vector3 worldShift = parent.TransformPoint(new Vector3(0f, localBottomY, 0f)) - new Vector3(b.center.x, b.min.y, b.center.z);
        go.transform.position += worldShift;
        return go;
    }

    // Replaces the placeholder robot capsule on the player with the robot model from Resources/Robot.
    public static void DressPlayer(GameObject player)
    {
        if (ModelSkin.Attach(player.transform, "Robot", 1.6f, -0.8f, true, new Vector3(-90f, 180f, -90f), new Vector3(0f, 0f, -180f)) == null) return;
        foreach (var r in player.GetComponentsInChildren<Renderer>(true))
            if (r.name == "RobotBody" || r.name == "RobotFront") Object.Destroy(r.gameObject);
    }

    static Transform Pivot(string name, Transform parent, Vector3 euler)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = euler;
        return t;
    }

    static Bounds Bounds(Renderer[] renderers)
    {
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static Material GetMaterial(string folder, string name)
    {
        string key = folder + "/" + name;
        if (materials.TryGetValue(key, out var m) && m != null) return m;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        m = new Material(shader) { name = name + "_Mat" };
        var tex = Resources.Load<Texture2D>(key);
        if (tex != null)
        {
            m.mainTexture = tex;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.25f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        materials[key] = m;
        return m;
    }
}
