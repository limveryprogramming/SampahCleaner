using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    [SerializeField] int count = 15;
    [SerializeField] float areaRadius = 6f;
    [SerializeField] float minDistanceFromCenter = 1.5f;

    void Start()
    {
        if (prefabs == null || prefabs.Length == 0) return;
        for (int i = 0; i < count; i++)
        {
            var dir = Random.insideUnitCircle.normalized * Random.Range(minDistanceFromCenter, areaRadius);
            var pos = transform.position + new Vector3(dir.x, 0f, dir.y);
            var rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, rot, transform);
        }
    }
}
