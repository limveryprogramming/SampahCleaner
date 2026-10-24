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

    public static Trash Spawn(Vector3 position)
    {
        var prefab = Resources.Load<GameObject>("Trash");
        if (prefab != null)
        {
            var instance = Instantiate(prefab, new Vector3(position.x, 0.2f, position.z),
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            instance.name = "Trash";
            if (!instance.TryGetComponent(out Trash t)) t = instance.AddComponent<Trash>();
            return t;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Trash";
        go.transform.position = new Vector3(position.x, 0.2f, position.z);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);
        go.GetComponent<Collider>().isTrigger = true;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        try { go.tag = Tag; }
        catch (UnityException) { Debug.LogError($"Tag '{Tag}' is missing. Add it in Project Settings > Tags and Layers."); }
        return go.AddComponent<Trash>();
    }
}
