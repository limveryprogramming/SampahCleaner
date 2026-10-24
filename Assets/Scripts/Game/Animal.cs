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
    }

    void Update()
    {
        if (IsDead) return;

        int nearby = 0;
        foreach (var t in Trash.All)
            if ((t.transform.position - transform.position).sqrMagnitude < poisonRadius * poisonRadius) nearby++;

        if (nearby > 0) exposure += nearby * Time.deltaTime;
        else exposure = Mathf.Max(0f, exposure - Time.deltaTime * 0.5f);

        rend.material.color = Color.Lerp(new Color(0.6f, 0.15f, 0.7f), baseColor, Health01);

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
        rend.material.color = new Color(0.5f, 0.05f, 0.05f);
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
        GameHud.Flash("An animal died from the pollution!");
        GameState.AddDeadAnimal();
    }
}
