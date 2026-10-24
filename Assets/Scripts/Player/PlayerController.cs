using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float collectRadius = 1.8f;
    public float photoRange = 30f;
    public float lookSensitivity = 0.12f;

    public bool InCameraMode { get; private set; }
    public bool ShopOpen { get; private set; }

    Camera cam;
    float yaw, pitch;
    float photoCooldown;

    void Start()
    {
        cam = Camera.main;
        transform.position = new Vector3(0f, 1f, 0f);
    }

    void Update()
    {
        if (GameState.Ending != EndingType.None) return;
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame && !InCameraMode) ShopOpen = !ShopOpen;
        if (kb.fKey.wasPressedThisFrame && !ShopOpen) SetCameraMode(!InCameraMode);
        if (ShopOpen) return;

        photoCooldown -= Time.deltaTime;

        if (InCameraMode)
        {
            if (mouse != null)
            {
                Vector2 d = mouse.delta.ReadValue();
                yaw += d.x * lookSensitivity;
                pitch = Mathf.Clamp(pitch - d.y * lookSensitivity, -40f, 40f);
            }
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cam.transform.position = transform.position + Vector3.up * 0.8f;
            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            bool shoot = kb.spaceKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (shoot && photoCooldown <= 0f) TakePhoto();
            return;
        }

        Move(kb);
        if (kb.eKey.wasPressedThisFrame) CollectNearbyTrash();
        FollowCamera();
    }

    void Move(Keyboard kb)
    {
        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir.z += 1f;
        if (kb.sKey.isPressed) dir.z -= 1f;
        if (kb.dKey.isPressed) dir.x += 1f;
        if (kb.aKey.isPressed) dir.x -= 1f;
        if (dir.sqrMagnitude > 0f)
        {
            dir.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
            yaw = transform.eulerAngles.y;
            Vector3 p = transform.position + dir * moveSpeed * Time.deltaTime;
            p.x = Mathf.Clamp(p.x, -28f, 28f);
            p.z = Mathf.Clamp(p.z, -28f, 28f);
            transform.position = p;
        }
    }

    void FollowCamera()
    {
        Vector3 desired = transform.position + new Vector3(0f, 14f, -9f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, desired, 8f * Time.deltaTime);
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
            Quaternion.LookRotation(transform.position - desired), 8f * Time.deltaTime);
    }

    void SetCameraMode(bool on)
    {
        InCameraMode = on;
        pitch = 0f;
        yaw = transform.eulerAngles.y;
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    void CollectNearbyTrash()
    {
        for (int i = Trash.All.Count - 1; i >= 0; i--)
        {
            Trash t = Trash.All[i];
            Vector3 d = t.transform.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude <= collectRadius * collectRadius)
            {
                Destroy(t.gameObject);
                GameState.AddTrashCollected();
            }
        }
    }

    void TakePhoto()
    {
        photoCooldown = 0.6f;
        GameHud.FlashCamera();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, photoRange)) return;
        var tourist = hit.collider.GetComponentInParent<Tourist>();
        if (tourist == null || tourist.Reported) return;

        if (tourist.isLitterbug && tourist.HasLittered)
        {
            tourist.MarkReported();
            GameState.AddLitterbugReported();
            GameHud.Flash("Valid report: litterbug photographed!");
        }
        else
        {
            GameHud.Flash("False report! That tourist did nothing wrong.");
            GameState.AddFalseReport();
        }
    }
}
