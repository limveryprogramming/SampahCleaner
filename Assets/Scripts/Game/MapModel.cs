using UnityEngine;

// Loads a diorama FBX from Resources, fits it to the play area and lowers/raises it so its ground sits at y = 0.
public static class MapModel
{
    public static GameObject Place(string resourcePath, Transform parent, float targetWidth, string materialPath = null)
    {
        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"Map model '{resourcePath}' not found in Resources; keeping placeholder ground.");
            return null;
        }

        var go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        var mat = materialPath != null ? Resources.Load<Material>(materialPath) : null;
        if (mat != null)
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return go;

        Bounds b = WorldBounds(renderers);

        // A diorama is a flat slab: its thinnest axis must be vertical. Stand it up if the import left it on its side.
        if (b.size.y > Mathf.Min(b.size.x, b.size.z) * 1.05f)
        {
            go.transform.rotation = b.size.z <= b.size.x ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.Euler(0f, 0f, 90f);
            // The flat base has many vertices at its lowest plane; if more sit on top, flip it.
            if (TopHeavy(go)) go.transform.rotation = Quaternion.Euler(180f, 0f, 0f) * go.transform.rotation;
            b = WorldBounds(renderers);
        }

        float scale = targetWidth / Mathf.Max(b.size.x, b.size.z);
        go.transform.localScale = Vector3.one * scale;
        go.transform.position = new Vector3(-b.center.x * scale, 0f, -b.center.z * scale);

        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
            if (mf.GetComponent<Collider>() == null) mf.gameObject.AddComponent<MeshCollider>();
        Physics.SyncTransforms();

        go.transform.position += Vector3.up * -GroundHeight(go.transform, targetWidth);
        Physics.SyncTransforms();
        return go;
    }

    static Bounds WorldBounds(Renderer[] renderers)
    {
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static bool TopHeavy(GameObject go)
    {
        float min = float.MaxValue, max = float.MinValue;
        var ys = new System.Collections.Generic.List<float>();
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null || !mf.sharedMesh.isReadable) continue;
            foreach (var v in mf.sharedMesh.vertices)
            {
                float y = mf.transform.TransformPoint(v).y;
                ys.Add(y); min = Mathf.Min(min, y); max = Mathf.Max(max, y);
            }
        }
        if (ys.Count == 0) return false;
        float tol = (max - min) * 0.02f;
        int low = 0, high = 0;
        foreach (float y in ys) { if (y < min + tol) low++; else if (y > max - tol) high++; }
        return high > low;
    }

    // Lowest top-surface height over a grid: the model's floor, ignoring buildings and props.
    static float GroundHeight(Transform model, float width)
    {
        float lowest = float.MaxValue;
        const int steps = 9;
        for (int ix = 0; ix < steps; ix++)
        for (int iz = 0; iz < steps; iz++)
        {
            float x = Mathf.Lerp(-width * 0.4f, width * 0.4f, ix / (steps - 1f));
            float z = Mathf.Lerp(-width * 0.4f, width * 0.4f, iz / (steps - 1f));
            if (Physics.Raycast(new Vector3(x, 500f, z), Vector3.down, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.transform.IsChildOf(model))
                lowest = Mathf.Min(lowest, hit.point.y);
        }
        return lowest == float.MaxValue ? 0f : lowest;
    }
}
