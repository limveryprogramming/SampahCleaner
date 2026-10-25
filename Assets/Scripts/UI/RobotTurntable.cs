using UnityEngine;

public class RobotTurntable : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 45f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.unscaledDeltaTime, 0f, Space.World);
    }
}
