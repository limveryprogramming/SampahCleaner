using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float collectRadius = 1.8f;
    public float photoRange = 30f;
    public float lookSensitivity = 0.12f;
    public float photoCooldownTime = 0.6f;
    public float cameraDistance = 10f;
    public float orbitPitch = 35f;

    public Transform collectZone;
    public Transform attractZone;
    [Range(0f, 1f)] public float attractAlpha = 0.3f;
    public float attractSizeScale = 0.7f;
    public float attractWidthScale = 1f;
    public float attractLengthScale = 1f;

    public bool InCameraMode { get; private set; }
    public bool ShopOpen { get; private set; }

    Camera cam;
    float yaw, pitch;
    float photoCooldown;
    PlayerInputActions input;
    Renderer[] bodyRenderers;

    CharacterController cc;
    float verticalVelocity;
    const float Gravity = -20f;

    void Awake()
    {
        input = new PlayerInputActions();

        var rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        var oldCol = GetComponent<Collider>();
        if (oldCol != null && !(oldCol is CharacterController)) Destroy(oldCol);

        cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        cc.height = 1.6f;
        cc.radius = 0.4f;
        cc.center = Vector3.zero;
        cc.stepOffset = 0.3f;

        if (collectZone == null) collectZone = transform.Find("CollectZone");
        if (attractZone == null) attractZone = transform.Find("AttractZone");
        if (attractZone == null && collectZone != null)
        {
            var go = new GameObject("AttractZone");
            go.transform.SetParent(transform, false);
            go.AddComponent<BoxCollider>().isTrigger = true;
            var zoneRb = go.AddComponent<Rigidbody>();
            zoneRb.isKinematic = true;
            zoneRb.useGravity = false;
            go.AddComponent<AttractZone>();
            attractZone = go.transform;
        }
        if (attractZone != null && attractZone.TryGetComponent(out AttractZone az)) az.target = collectZone;
        SetupZoneVisuals();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();
    void OnDestroy() => input.Dispose();

    void Start()
    {
        cam = Camera.main;
        cc.enabled = false;
        transform.position = new Vector3(0f, 1.2f, 0f);
        cc.enabled = true;
        pitch = orbitPitch;
    }

    void Update()
    {
        UpdateZones();

        bool freeCursor = ShopOpen || GameState.Ending != EndingType.None;
        Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeCursor;

        if (GameState.Ending != EndingType.None) return;
        var mouse = Mouse.current;

        if (input.Player.OpenShop.WasPressedThisFrame() && !InCameraMode) ShopOpen = !ShopOpen;
        if (input.Player.OpenCamera.WasPressedThisFrame() && !ShopOpen) SetCameraMode(!InCameraMode);

        photoCooldown -= Time.deltaTime;
        if (ShopOpen || InCameraMode)
        {
            ApplyMovement(Vector3.zero);
            if (ShopOpen) return;
        }

        if (mouse != null)
        {
            Vector2 d = mouse.delta.ReadValue();
            yaw += d.x * lookSensitivity;
            pitch = InCameraMode
                ? Mathf.Clamp(pitch - d.y * lookSensitivity, -40f, 40f)
                : Mathf.Clamp(pitch - d.y * lookSensitivity, 5f, 70f);
        }

        if (InCameraMode)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cam.transform.position = transform.position + Vector3.up * 0.8f;
            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            if (input.Player.TakePhoto.WasPressedThisFrame() && photoCooldown <= 0f) TakePhoto();
            return;
        }

        Move(input.Player.MoveAction.ReadValue<Vector2>());
        FollowCamera();
    }

    public void SetShopOpen(bool open)
    {
        if (open && InCameraMode) return;
        ShopOpen = open;
    }

    void Move(Vector2 move)
    {
        Vector3 dir = new Vector3(move.x, 0f, move.y);
        if (dir.sqrMagnitude > 0f)
        {
            dir = Quaternion.Euler(0f, yaw, 0f) * dir.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
        }
        else dir = Vector3.zero;
        ApplyMovement(dir * moveSpeed);
    }

    void ApplyMovement(Vector3 horizontal)
    {
        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += Gravity * Time.deltaTime;
        cc.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);

        Vector3 p = transform.position;
        float cx = Mathf.Clamp(p.x, -28f, 28f), cz = Mathf.Clamp(p.z, -58f, 28f);
        if (cx != p.x || cz != p.z)
        {
            cc.enabled = false;
            transform.position = new Vector3(cx, p.y, cz);
            cc.enabled = true;
        }
    }

    void FollowCamera()
    {
        Vector3 pivot = transform.position + Vector3.up * 1.2f;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        cam.transform.position = pivot - rot * Vector3.forward * cameraDistance;
        cam.transform.rotation = rot;
    }

    void SetCameraMode(bool on)
    {
        InCameraMode = on;
        pitch = on ? 0f : orbitPitch;
        if (bodyRenderers == null) bodyRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in bodyRenderers)
        {
            if (r == null || r.transform == collectZone || r.transform == attractZone) continue;
            r.enabled = !on;
        }
    }

    void SetupZoneVisuals()
    {
        if (collectZone != null && collectZone.TryGetComponent(out Renderer cr)) cr.enabled = false;
        if (attractZone == null) return;

        var mr = attractZone.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tmp);
            attractZone.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            mr = attractZone.gameObject.AddComponent<MeshRenderer>();
        }
        mr.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.2f, 0.6f, 1f, attractAlpha) };
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    void UpdateZones()
    {
        float feet = -cc.height / 2f;
        if (collectZone != null)
        {
            const float size = 1.2f;
            collectZone.localScale = new Vector3(size, 1f, size);
            collectZone.localPosition = new Vector3(0f, feet + 0.5f, cc.radius + size / 2f);
        }
        if (attractZone != null)
        {
            float size = collectRadius * attractSizeScale;
            float length = size * attractLengthScale;
            attractZone.localScale = new Vector3(size * attractWidthScale, 1.5f, length);
            attractZone.localPosition = new Vector3(0f, feet + 0.75f, cc.radius + length / 2f);
        }
    }

    void TakePhoto()
    {
        photoCooldown = photoCooldownTime;
        GameHud.FlashCamera();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        var hits = Physics.RaycastAll(ray, photoRange, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        RaycastHit hit = default;
        bool found = false;
        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            hit = h;
            found = true;
            break;
        }
        if (!found) return;
        var ship = hit.collider.GetComponentInParent<Ship>();
        if (ship != null)
        {
            if (ship.isIndustrial) { GameState.AddShipPhoto(ship.shipId); GameHud.Flash("Valid report: industrial ship photographed!"); }
            else GameHud.Flash("That is a fishing boat, not an industrial polluter.");
            return;
        }

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
