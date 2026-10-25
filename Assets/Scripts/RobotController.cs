using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    [Tooltip("Yaw correction so the model's front faces the move direction. Land robot: -90. Trash robots: set by prefab (180).")]
    [SerializeField] float frontYawOffset = -90f;
    [SerializeField] float walkSpeed = 1.2f;
    [SerializeField] float runSpeed = 2.8f;
    [SerializeField] float turnSpeed = 540f;
    [SerializeField] float animDamp = 0.1f;
    [SerializeField] Transform cameraTransform;

    Animator animator;
    static readonly int SpeedHash = Animator.StringToHash("Speed");

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (animator == null || !animator.gameObject.activeInHierarchy)
            animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        var input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        bool running = kb.leftShiftKey.isPressed;
        float targetSpeed = input.sqrMagnitude > 0.01f ? (running ? runSpeed : walkSpeed) : 0f;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 dir = input;
            if (cameraTransform != null)
            {
                var fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                dir = (fwd * input.z + right * input.x).normalized;
            }
            var look = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(0f, frontYawOffset, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            transform.position += dir * targetSpeed * Time.deltaTime;
        }

        animator.SetFloat(SpeedHash, targetSpeed, animDamp, Time.deltaTime);
    }
}
