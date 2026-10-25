using UnityEngine;

public class Animal : MonoBehaviour
{
    public static int AliveCount;

    public float poisonRadius = 5f;
    public float timeToDie = 25f;
    public float wanderSpeed = 0.8f;

    float exposure;
    Vector3 target;
    Renderer rend;
    Color baseColor;

    public bool IsDead { get; private set; }
    public float Health01 => Mathf.Clamp01(1f - exposure / timeToDie);

    void OnEnable() => AliveCount++;
    void OnDisable() { if (!IsDead) AliveCount--; }

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
        target = transform.position;

        var all = GetComponentsInChildren<Renderer>();
        var list = new System.Collections.Generic.List<Renderer>();
        foreach (var r in all) if (r != rend) list.Add(r);
        skins = list.ToArray();
        if (skins.Length > 0) rend.enabled = false;

        foreach (var s in skins)
        {
            var m = s.material;
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", new Color(0.35f, 0.35f, 0.3f));
        }

        var lightGo = new GameObject("Highlight");
        lightGo.transform.SetParent(transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        glow = lightGo.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.95f, 0.6f);
        glow.range = 6f;
        glow.intensity = 3f;
    }

    Light glow;

    Renderer[] skins;

    void Tint(Color poisoned, Color healthy, float t)
    {
        rend.material.color = Color.Lerp(poisoned, healthy, t);
        foreach (var s in skins) s.material.color = Color.Lerp(poisoned, Color.white, t);
    }

    void Update()
    {
        if (IsDead) return;

        int nearby = 0;
        foreach (var t in Trash.All)
            if ((t.transform.position - transform.position).sqrMagnitude < poisonRadius * poisonRadius) nearby++;

        if (nearby > 0) exposure += nearby * Time.deltaTime;
        else exposure = Mathf.Max(0f, exposure - Time.deltaTime * 0.5f);

        Tint(new Color(0.6f, 0.15f, 0.7f), baseColor, Health01);

        if (exposure >= timeToDie) { Die(); return; }

        if ((target - transform.position).sqrMagnitude < 0.1f)
            target = transform.position + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        target.x = Mathf.Clamp(target.x, -27f, 27f);
        target.z = Mathf.Clamp(target.z, -27f, 22f);
        transform.position = Vector3.MoveTowards(transform.position, target, wanderSpeed * Time.deltaTime);
    }

    void Die()
    {
        IsDead = true;
        AliveCount--;
        if (glow != null) glow.enabled = false;
        foreach (var s in skins)
            if (s.material.HasProperty("_EmissionColor")) s.material.SetColor("_EmissionColor", Color.black);
        Tint(new Color(0.5f, 0.05f, 0.05f), new Color(0.5f, 0.05f, 0.05f), 0f);
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        GameHud.Flash("An animal died from the pollution!");
        GameState.AddDeadAnimal();
    }
}
