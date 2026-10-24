using UnityEngine;

[RequireComponent(typeof(EvolvingRobot))]
public class TrashCollector : MonoBehaviour
{
    [SerializeField] float pickupRadius = 0.6f;

    EvolvingRobot robot;

    void Awake() => robot = GetComponent<EvolvingRobot>();

    void Update()
    {
        float r2 = pickupRadius * pickupRadius;
        var origin = transform.position;
        for (int i = TrashItem.All.Count - 1; i >= 0; i--)
        {
            var item = TrashItem.All[i];
            var d = item.transform.position - origin;
            d.y = 0f;
            if (d.sqrMagnitude > r2) continue;
            robot.AddTrash(item.Value);
            Destroy(item.gameObject);
        }
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(16, 12, 400, 32), "Trash: " + robot.Trash + "   Level: " + (robot.Level + 1), style);
    }
}
