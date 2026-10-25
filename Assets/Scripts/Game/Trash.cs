using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public const string Tag = "Rubbish";

    public static readonly List<Trash> All = new List<Trash>();

    public float attractSpeed = 12f;

    Transform attractTarget;
    bool consumed;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public void AttractTo(Transform target) => attractTarget = target;

    // Destroys this trash once; returns false if it was already consumed this frame.
    public bool TryConsume()
    {
        if (consumed) return false;
        consumed = true;
        Destroy(gameObject);
        return true;
    }

    void Update()
    {
        if (attractTarget == null) return;
        Vector3 goal = new Vector3(attractTarget.position.x, transform.position.y, attractTarget.position.z);
        transform.position = Vector3.MoveTowards(transform.position, goal, attractSpeed * Time.deltaTime);
    }

    // Lowest upward-facing surface under (x, z) that is low enough to be ground (not a roof or tree top).
    public static float GroundBase;
    const float GroundBand = 2f;

    public static float GroundY(float x, float z)
    {
        float best = float.MaxValue;
        foreach (var hit in Physics.RaycastAll(new Vector3(x, 300f, z), Vector3.down, 600f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider is CharacterController || hit.collider.name.StartsWith("BoundWall")
                || hit.collider.GetComponentInParent<Trash>() != null) continue;
            if (hit.normal.y > 0.5f && hit.point.y < GroundBase + GroundBand) best = Mathf.Min(best, hit.point.y);
        }
        return best == float.MaxValue ? GroundBase : best;
    }

    public static bool OceanOnly;
    const float OceanTolerance = 0.6f;

    public static bool IsOcean(float x, float z) => GroundY(x, z) <= GroundBase + OceanTolerance;

    // Returns null when OceanOnly is set and the spot is land/reef.
    public static Trash Spawn(Vector3 position)
    {
        float gy = GroundY(position.x, position.z);
        if (OceanOnly && gy > GroundBase + OceanTolerance) return null;
        position.y = gy + 0.6f;
        var prefab = Resources.Load<GameObject>("Trash");
        if (prefab != null && Resources.LoadAll<GameObject>("TrashModels").Length == 0)
        {
            var instance = Instantiate(prefab, position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            instance.name = "Trash";
            if (!instance.TryGetComponent(out Trash t)) t = instance.AddComponent<Trash>();
            return t;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Trash";
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);
        go.GetComponent<Collider>().isTrigger = true;
        // World size ~0.7 units; the cube's bottom is at local y = -0.5.
        if (ModelSkin.Attach(go.transform, "TrashModels", 0.7f, -0.5f, false) != null)
            go.GetComponent<Renderer>().enabled = false;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        try { go.tag = Tag; }
        catch (UnityException) { Debug.LogError($"Tag '{Tag}' is missing. Add it in Project Settings > Tags and Layers."); }
        return go.AddComponent<Trash>();
    }
}
