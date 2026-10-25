using UnityEngine;

public class Ship : MonoBehaviour
{
    public bool isIndustrial;
    public string shipId;
    public float speed = 3f;
    public Vector2 dropInterval = new Vector2(4f, 7f);
    public int dropsPerPile = 4;
    public int maxTrashOnMap = 60;
    public float exitX = 42f;

    float direction = 1f;
    float nextDrop;

    void Start()
    {
        direction = transform.position.x < 0f ? 1f : -1f;
        transform.rotation = Quaternion.LookRotation(Vector3.right * direction);
        nextDrop = Time.time + Random.Range(dropInterval.x * 0.5f, dropInterval.y);
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        if (Time.time >= nextDrop)
        {
            nextDrop = Time.time + Random.Range(dropInterval.x, dropInterval.y);
            DropPile();
        }

        if (Mathf.Abs(transform.position.x) > exitX) Destroy(gameObject);
    }

    void DropPile()
    {
        Vector3 stern = transform.position - transform.forward * 2f;
        for (int i = 0; i < dropsPerPile && Trash.All.Count < maxTrashOnMap; i++)
            Trash.Spawn(stern + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f)));
    }
}
