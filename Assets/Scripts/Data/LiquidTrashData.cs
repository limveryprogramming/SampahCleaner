using UnityEngine;

[CreateAssetMenu(fileName = "NewLiquidTrash", menuName = "SampahCleaner/Liquid Trash Data")]
public class LiquidTrashData : TrashData
{
    [Header("Liquid")]
    public float startRadius = 0.5f;
    public float maxRadius = 3f;
    [Tooltip("Units per second the puddle grows until it reaches max radius.")]
    public float spreadSpeed = 0.3f;
    [Tooltip("Seconds until the puddle disappears on its own. 0 = never.")]
    public float evaporateTime = 0f;
    [Range(0.1f, 1f)] public float playerSlowMultiplier = 0.6f;
    [Tooltip("Needs a special tool to clean instead of being picked up by hand.")]
    public bool requiresSpecialTool = true;

    void OnValidate()
    {
        maxRadius = Mathf.Max(maxRadius, startRadius);
    }
}
