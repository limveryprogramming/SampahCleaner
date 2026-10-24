using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

public static class PlayerCameraSetup
{
    // Trigger events need a Rigidbody on one of the two objects, so the zone gets a kinematic one.
    static void MakeTriggerZone(GameObject go)
    {
        var col = go.GetComponent<BoxCollider>();
        if (col == null) col = Undo.AddComponent<BoxCollider>(go);
        col.isTrigger = true;
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = Undo.AddComponent<Rigidbody>(go);
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    [MenuItem("Tools/Setup Player And Camera")]
    static void Setup()
    {
        PlayerInputSetup.Setup();

        // Player
        var controller = Object.FindFirstObjectByType<PlayerController>();
        GameObject player = controller != null ? controller.gameObject : GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Setup Player And Camera");
            Debug.Log("Created Player object.");
        }
        if (player.GetComponent<PlayerController>() == null)
        {
            Undo.AddComponent<PlayerController>(player);
            Debug.Log("Added PlayerController to Player.");
        }
        Undo.RecordObject(player.transform, "Setup Player And Camera");
        player.transform.position = new Vector3(0f, 1.2f, 0f);

        // CharacterController handles walking on colliders (beach, sea) with gravity; a Rigidbody or extra collider would conflict
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) Undo.DestroyObjectImmediate(rb);
        var oldCol = player.GetComponent<Collider>();
        if (oldCol != null && !(oldCol is CharacterController)) Undo.DestroyObjectImmediate(oldCol);
        var cc = player.GetComponent<CharacterController>();
        if (cc == null) cc = Undo.AddComponent<CharacterController>(player);
        cc.height = 1.6f;
        cc.radius = 0.4f;
        cc.center = Vector3.zero;
        cc.stepOffset = 0.3f;

        // Robot body (child visuals are hidden in first-person camera mode)
        bool hasBody = false;
        foreach (var rend in player.GetComponentsInChildren<Renderer>())
            if (rend.name != "CollectZone") hasBody = true;
        if (!hasBody)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "RobotBody";
            Undo.RegisterCreatedObjectUndo(body, "Setup Player And Camera");
            body.transform.SetParent(player.transform, false);
            body.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.7f, 0.3f);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "RobotFront";
            Undo.RegisterCreatedObjectUndo(nose, "Setup Player And Camera");
            nose.transform.SetParent(player.transform, false);
            nose.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            nose.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            Debug.Log("Created RobotBody placeholder.");
        }

        // Red collect zone in front of the player (size and position are driven by PlayerController)
        var pc = player.GetComponent<PlayerController>();
        var zone = player.transform.Find("CollectZone");
        if (zone == null)
        {
            var zoneGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zoneGo.name = "CollectZone";
            Undo.RegisterCreatedObjectUndo(zoneGo, "Setup Player And Camera");
            zoneGo.transform.SetParent(player.transform, false);
            zone = zoneGo.transform;
            Debug.Log("Created CollectZone.");
        }

        if (!InternalEditorUtility.tags.Contains(Trash.Tag))
        {
            InternalEditorUtility.AddTag(Trash.Tag);
            Debug.Log($"Added tag '{Trash.Tag}'.");
        }

        MakeTriggerZone(zone.gameObject);
        if (zone.GetComponent<CollectZone>() == null) Undo.AddComponent<CollectZone>(zone.gameObject);
        var zr = zone.GetComponent<Renderer>();
        if (zr != null) zr.sharedMaterial.color = new Color(1f, 0.1f, 0.1f);

        // Larger invisible attract zone (blue wireframe in the Scene view) that pulls trash toward the collect zone
        var attract = player.transform.Find("AttractZone");
        if (attract == null)
        {
            var attractGo = new GameObject("AttractZone");
            Undo.RegisterCreatedObjectUndo(attractGo, "Setup Player And Camera");
            attractGo.transform.SetParent(player.transform, false);
            attract = attractGo.transform;
            Debug.Log("Created AttractZone.");
        }
        MakeTriggerZone(attract.gameObject);
        var attractScript = attract.GetComponent<AttractZone>();
        if (attractScript == null) attractScript = Undo.AddComponent<AttractZone>(attract.gameObject);
        attractScript.target = zone;

        Undo.RecordObject(pc, "Setup Player And Camera");
        pc.collectZone = zone;
        pc.attractZone = attract;

        // Camera
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            Undo.RegisterCreatedObjectUndo(camGo, "Setup Player And Camera");
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
            Debug.Log("Created Main Camera.");
        }
        if (cam.GetComponent<AudioListener>() == null) Undo.AddComponent<AudioListener>(cam.gameObject);
        Undo.RecordObject(cam.transform, "Setup Player And Camera");
        cam.transform.position = player.transform.position + new Vector3(0f, 6f, -8f);
        cam.transform.LookAt(player.transform.position + Vector3.up * 1.2f);
        cam.nearClipPlane = 0.1f;

        // Warn about stray cameras that would fight over the screen
        foreach (var other in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (other != cam && other.enabled)
                Debug.LogWarning($"Extra enabled camera '{other.name}' found. Disable it so only Main Camera renders.", other);

        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeObject = player;
        Debug.Log("Player and camera setup done. Save the scene (Ctrl+S).");
    }
}
