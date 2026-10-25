using UnityEngine;

// Large trigger in front of the player. Trash that touches it is pulled toward the target (the CollectZone).
public class AttractZone : MonoBehaviour
{
    public Transform target;

    void OnTriggerEnter(Collider other) => Pull(other);
    void OnTriggerStay(Collider other) => Pull(other);

    void Pull(Collider other)
    {
        if (target == null || other.tag != Trash.Tag) return;
        if (other.TryGetComponent(out Trash trash)) trash.AttractTo(target);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
