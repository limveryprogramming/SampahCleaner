using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public static readonly List<Trash> All = new List<Trash>();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static Trash Spawn(Vector3 position)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Trash";
        go.transform.position = new Vector3(position.x, 0.2f, position.z);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);
        Destroy(go.GetComponent<Collider>());
        return go.AddComponent<Trash>();
    }
}
