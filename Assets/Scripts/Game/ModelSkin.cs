using System.Collections.Generic;
using UnityEngine;

// Dresses a placeholder object with a random textured FBX from Resources/<folder>.
public static class ModelSkin
{
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
    static readonly Dictionary<string, GameObject[]> cache = new Dictionary<string, GameObject[]>();

    // Returns the attached model, or null if the folder has none. Model is scaled so its height (or longest side
    // if keepLongest) equals targetSize, centred on x/z, with its lowest point at localBottomY.
    public static GameObject Attach(Transform parent, string folder, float targetSize, float localBottomY, bool standUp)
    {
        if (!cache.TryGetValue(folder, out var prefabs)) cache[folder] = prefabs = Resources.LoadAll<GameObject>(folder);
        if (prefabs.Length == 0) return null;

        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        var go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var mat = GetMaterial(folder, prefab.name);
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) { r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
        if (renderers.Length == 0) return go;

        Bounds b = Bounds(renderers);
        if (standUp && b.size.y < Mathf.Min(b.size.x, b.size.z) * 0.9f)
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
