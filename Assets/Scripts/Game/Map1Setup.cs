using UnityEngine;
using UnityEngine.SceneManagement;

// Builds Map 1 from primitives (cubes/capsules) as placeholders until real assets exist.
public static class Map1Setup
{
    const string SceneName = "Map1";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => { if (scene.name == SceneName) Build(); };
        if (SceneManager.GetActiveScene().name == SceneName) Build();
    }

    static void Build()
    {
        GameState.Reset();
        Trash.All.Clear();
        Tourist.All.Clear();
        Animal.AliveCount = 0;

        var root = new GameObject("Map1Generated").transform;

        var ground = Prim(PrimitiveType.Cube, "Beach", new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60), new Color(0.93f, 0.85f, 0.6f), root);
        ground.GetComponent<Collider>().isTrigger = false;
        Prim(PrimitiveType.Cube, "Sea", new Vector3(0, -0.6f, -45), new Vector3(60, 1, 30), new Color(0.15f, 0.45f, 0.8f), root);

        if (MapModel.Place("Maps/BeachTown/Meshy_AI_diorama_beach_town_ti_1024110804_texture", root, 60f, "Maps/BeachTown/BeachTown_Mat", false) != null)
        {
            ground.GetComponent<Renderer>().enabled = false;
            root.Find("Sea").GetComponent<Renderer>().enabled = false;
        }

        // The placeholder floor stays as an invisible flat walkway at y = 0 so dunes and rubble in the model never block the player.
        // The model supplies its own boundary walls, so the old placeholder rails are only built without a model.
        if (root.Find("Meshy_AI_diorama_beach_town_ti_1024110804_texture") == null)
        {
            Prim(PrimitiveType.Cube, "WallN", new Vector3(0, 0.5f, 30), new Vector3(60, 1, 0.5f), new Color(0.4f, 0.3f, 0.2f), root);
            Prim(PrimitiveType.Cube, "WallE", new Vector3(30, 0.5f, 0), new Vector3(0.5f, 1, 60), new Color(0.4f, 0.3f, 0.2f), root);
            Prim(PrimitiveType.Cube, "WallW", new Vector3(-30, 0.5f, 0), new Vector3(0.5f, 1, 60), new Color(0.4f, 0.3f, 0.2f), root);
        }

        // Switch off every boundary collider (the red gizmo boxes) so nothing invisible blocks the player.
        foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            string n = col.gameObject.name;
            if (n.Contains("Bound") || n.Contains("Wall")) col.enabled = false;
        }

        PlayerController.SpawnOverride = new Vector3(17f, 1f, 9f);

        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null && pc.TryGetComponent<CharacterController>(out var cc))
        {
            cc.stepOffset = 0.6f;
            cc.slopeLimit = 60f;
        }

        // Player: add a visible body if the scene has none
        var player = FindPlayer();
        if (player != null)
        {
            bool hasBody = false;
            foreach (var rend in player.GetComponentsInChildren<Renderer>())
                if (rend.name != "CollectZone") hasBody = true;
            if (!hasBody)
            {
                var visual = Prim(PrimitiveType.Capsule, "RobotBody", Vector3.zero, Vector3.one, new Color(0.2f, 0.7f, 0.3f), player.transform);
                visual.transform.localPosition = Vector3.zero;
                Object.Destroy(visual.GetComponent<Collider>());
                var nose = Prim(PrimitiveType.Cube, "RobotFront", Vector3.zero, new Vector3(0.4f, 0.3f, 0.4f), Color.black, player.transform);
                nose.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
                Object.Destroy(nose.GetComponent<Collider>());
            }
        }

        // Tourists: 7 total, 3 of them litterbugs
        for (int i = 0; i < 7; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-22f, 22f), 1f, Random.Range(-5f, 22f));
            bool bug = i < 3;
            var t = Prim(PrimitiveType.Capsule, bug ? "Tourist_Litterbug" : "Tourist", pos, Vector3.one,
                bug ? new Color(0.85f, 0.4f, 0.2f) : new Color(0.3f, 0.5f, 0.9f), root);
            var tourist = t.AddComponent<Tourist>();
            tourist.isLitterbug = bug;
        }

        // Animals: 6 cubes
        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-24f, 24f), 0.3f, Random.Range(-20f, 18f));
            var a = Prim(PrimitiveType.Cube, "Animal", pos, new Vector3(0.8f, 0.6f, 1.2f), new Color(0.9f, 0.6f, 0.75f), root);
            Object.Destroy(a.GetComponent<Collider>());
            a.AddComponent<Animal>();
        }

        // Starting litter
        for (int i = 0; i < 6; i++)
            Trash.Spawn(new Vector3(Random.Range(-22f, 22f), 0f, Random.Range(-10f, 22f)));

        new GameObject("HUD").AddComponent<GameHud>();    }

    // First flat spot on the model (searching outward from the centre) to drop the player onto.
    static Vector3? FindSandSpawn(Transform root)
    {
        var model = root.Find("Meshy_AI_diorama_beach_town_ti_1024110804_texture");
        if (model == null) return null;
        for (float r = 0f; r <= 24f; r += 3f)
        for (int a = 0; a < (r == 0f ? 1 : 8); a++)
        {
            float ang = a * Mathf.PI / 4f;
            var o = new Vector3(Mathf.Cos(ang) * r, 100f, Mathf.Sin(ang) * r);
            if (Physics.Raycast(o, Vector3.down, out var hit, 200f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.transform.IsChildOf(model) && hit.normal.y > 0.9f && hit.point.y < 1.5f)
                return hit.point + Vector3.up * 1.2f;
        }
        return null;
    }

    static GameObject FindPlayer()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        return pc != null ? pc.gameObject : null;
    }

    static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color, Transform parent)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        return go;
    }
}
