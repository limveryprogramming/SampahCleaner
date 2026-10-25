using UnityEngine;

[CreateAssetMenu(fileName = "NewTrash", menuName = "SampahCleaner/Trash Data")]
public class TrashData : ScriptableObject
{
    [Header("General")]
    public string displayName = "Trash";
    [TextArea] public string description;
    public Sprite icon;
    public GameObject prefab;
    public Color placeholderColor = new Color(0.45f, 0.3f, 0.15f);
    public Vector3 placeholderScale = new Vector3(0.4f, 0.4f, 0.4f);

    [Header("Gameplay")]
    public int sellValue = 1;
    public float collectTime = 0f;

    [Header("Pollution")]
    [Tooltip("How much this trash poisons nearby animals. 1 = normal.")]
    public float poisonStrength = 1f;
    public float poisonRadius = 5f;
}
