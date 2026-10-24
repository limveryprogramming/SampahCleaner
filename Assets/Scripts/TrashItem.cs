using System.Collections.Generic;
using UnityEngine;

public class TrashItem : MonoBehaviour
{
    public static readonly List<TrashItem> All = new List<TrashItem>();

    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 60f;
    [SerializeField] float bobHeight = 0.03f;
    [SerializeField] float bobSpeed = 2f;

    public int Value => value;

    Vector3 startPos;
    float phase;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Start()
    {
        startPos = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        var p = startPos;
        p.y += Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
        transform.position = p;
    }
}
