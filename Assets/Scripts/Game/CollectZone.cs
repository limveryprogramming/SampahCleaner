using UnityEngine;

// Small red trigger in front of the player. Trash that touches it is destroyed and counted.
public class CollectZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other) => Collect(other);
    void OnTriggerStay(Collider other) => Collect(other);

    void Collect(Collider other)
    {
        if (other.tag != Trash.Tag || !other.TryGetComponent(out Trash trash)) return;
        if (!trash.TryConsume()) return;

        GameState.AddTrashCollected();
        GameHud.Flash($"Trash collected: {GameState.TrashCollected}  (+{GameState.CoinsPerTrash} coin)");
    }
}
