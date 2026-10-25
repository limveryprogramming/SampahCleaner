using System.Collections.Generic;
using UnityEngine;

public class Tourist : MonoBehaviour
{
    public static readonly List<Tourist> All = new List<Tourist>();

    public bool isLitterbug;
    public float speed = 2f;
    public Vector2 litterInterval = new Vector2(5f, 9f);
    public int maxTrashOnMap = 60;
    public Color normalColor = new Color(0.2f, 0.45f, 1f);
    public Color litterbugColor = new Color(1f, 0.15f, 0.15f);

    public bool HasLittered { get; private set; }
    public bool Reported { get; private set; }

    Vector3 target;
    float nextLitter;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Start()
    {
        var rend = GetComponentInChildren<Renderer>();
        // Capsule is 2 tall and centred on the tourist, so feet are at local y = -1.
        var model = ModelSkin.Attach(transform, "Characters", 1.8f, -1f, true);
        if (model != null)
        {
            if (rend != null) rend.enabled = false;
        }
        else if (rend != null) rend.material.color = isLitterbug ? litterbugColor : normalColor;
        target = transform.position;
        nextLitter = Time.time + Random.Range(litterInterval.x * 0.5f, litterInterval.y);
    }

    void Update()
    {
        if (Reported) { Leave(); return; }

        if ((target - transform.position).sqrMagnitude < 0.25f)
            target = new Vector3(Random.Range(-24f, 24f), 1f, Random.Range(-24f, 20f));

        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
            transform.position += dir.normalized * speed * Time.deltaTime;
        }

        if (isLitterbug && Time.time >= nextLitter)
        {
            nextLitter = Time.time + Random.Range(litterInterval.x, litterInterval.y);
            if (Trash.All.Count < maxTrashOnMap)
            {
                Trash.Spawn(transform.position + transform.forward * 0.6f);
                HasLittered = true;
            }
        }
    }

    public void MarkReported() => Reported = true;

    void Leave()
    {
        Vector3 exit = new Vector3(transform.position.x, 1f, -40f);
        transform.position = Vector3.MoveTowards(transform.position, exit, 5f * Time.deltaTime);
        if (transform.position.z < -29f) Destroy(gameObject);
    }
}
