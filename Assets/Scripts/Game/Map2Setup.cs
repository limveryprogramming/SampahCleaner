using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds Map 2 (open sea) from primitives as placeholders until real assets exist.
public static class Map2Setup
{
    const string SceneName = "Map2";
    const float FirstShipTime = 30f;
    static readonly Vector2 ShipInterval = new Vector2(35f, 60f);

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

        var root = new GameObject("Map2Generated").transform;

        var sea = Prim(PrimitiveType.Cube, "Sea", new Vector3(0, -0.5f, -15), new Vector3(60, 1, 90), new Color(0.1f, 0.4f, 0.75f), root);
        var dock = Prim(PrimitiveType.Cube, "Dock", new Vector3(0, -0.3f, 26), new Vector3(14, 0.6f, 6), new Color(0.45f, 0.32f, 0.2f), root);

        if (MapModel.Place("Maps/OceanReef/Meshy_AI_diorama_ocean_reef_ti_1024110834_texture", root, 60f, "Maps/OceanReef/OceanReef_Mat") != null)
        {
            sea.GetComponent<Renderer>().enabled = false;
            dock.GetComponent<Renderer>().enabled = false;
        }

        SeaLevel = MedianSurface();
        Trash.GroundBase = SeaLevel;
        PlayerController.SpawnOverride = new Vector3(0f, MapModel.SurfaceY(0f, 0f, SeaLevel) + 1.2f, 0f);

        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) AddPlayerBody(player.gameObject);

        for (int i = 0; i < 3; i++)
        {
            float ax = Random.Range(-24f, 24f), az = Random.Range(-25f, 20f);
            Vector3 pos = new Vector3(ax, Trash.GroundY(ax, az) + 0.5f, az);
            var a = Prim(PrimitiveType.Cube, "SeaAnimal", pos, Vector3.one, new Color(0.3f, 0.8f, 0.7f), root);
            Object.Destroy(a.GetComponent<Collider>());
            ModelSkin.Attach(a.transform, "Turtle", 1.4f, -0.5f, false, new Vector3(-90f, 0f, 0f));
            a.AddComponent<Animal>();
        }

        Trash.OceanOnly = true;
        for (int spawned = 0, tries = 0; spawned < 70 && tries < 700; tries++)
            if (Trash.Spawn(new Vector3(Random.Range(-26f, 26f), 0f, Random.Range(-50f, 24f))) != null) spawned++;

        new GameObject("HUD").AddComponent<GameHud>();
        new GameObject("ShipSpawner").AddComponent<ShipSpawnRunner>().StartCoroutine(ShipSchedule(root));
    }

    static float SeaLevel;

    // Typical surface height across the model: the water/seabed plane that most things should sit on.
    static float MedianSurface()
    {
        var ys = new List<float>();
        for (float x = -24f; x <= 24f; x += 6f)
        for (float z = -24f; z <= 24f; z += 6f)
        {
            float y = MapModel.SurfaceY(x, z, float.NaN);
            if (!float.IsNaN(y)) ys.Add(y);
        }
        if (ys.Count == 0) return 0f;
        ys.Sort();
        return ys[ys.Count / 2];
    }

    static IEnumerator ShipSchedule(Transform root)
    {
        var order = new List<bool> { true, true, true, false, false };
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        yield return new WaitForSeconds(FirstShipTime);
        int industrial = 0;
        foreach (bool isIndustrial in order)
        {
            SpawnShip(isIndustrial, isIndustrial ? "Industrial_" + (++industrial) : "Fishing_" + Time.time, root);
            yield return new WaitForSeconds(Random.Range(ShipInterval.x, ShipInterval.y));
        }
    }

    // Unscaled holder so the model is not stretched; the box collider keeps the ship photographable.
    static GameObject BuildModelShip(string name, string folder, float length, Vector3 pos, Transform root)
    {
        var holder = new GameObject(name);
        holder.transform.SetParent(root, false);
        holder.transform.position = pos;
        var model = ModelSkin.Attach(holder.transform, folder, length, -0.8f, false, new Vector3(270f, 0f, 0f));
        if (model == null) { Object.Destroy(holder); return null; }

        // Ships travel along local z; turn the model if its long side runs along x.
        var rs = model.GetComponentsInChildren<Renderer>();
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        if (b.size.x > b.size.z) model.transform.RotateAround(holder.transform.position, Vector3.up, 90f);

        b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        var box = holder.AddComponent<BoxCollider>();
        box.center = holder.transform.InverseTransformPoint(b.center);
        box.size = b.size;
        return holder;
    }

    static void SpawnShip(bool isIndustrial, string id, Transform root)
    {
        bool fromLeft = Random.value < 0.5f;
        Vector3 pos = new Vector3(fromLeft ? -40f : 40f, SeaLevel + (isIndustrial ? 0.8f : 1.8f), Random.Range(-45f, 10f));

        GameObject ship = BuildModelShip(isIndustrial ? "Ship_Industrial" : "Ship_Fishing",
            isIndustrial ? "Ships" : "FishingBoats", isIndustrial ? 18f : 9f, pos, root);

        if (ship == null)
        {
            ship = Prim(PrimitiveType.Cube, isIndustrial ? "Ship_Industrial" : "Ship_Fishing", pos,
                isIndustrial ? new Vector3(5f, 1.6f, 14f) : new Vector3(2.5f, 1f, 7f),
                isIndustrial ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.75f, 0.3f, 0.2f), root);
            ship.transform.position = pos;

            float h = isIndustrial ? 2.4f : 1.4f;
            var cabin = Prim(PrimitiveType.Cube, "Cabin", Vector3.zero, new Vector3(0.6f, 0.9f, 0.25f), Color.white, ship.transform);
            cabin.transform.localPosition = new Vector3(0f, h / 2f, -0.25f);
        }

        var s = ship.AddComponent<Ship>();
        s.isIndustrial = isIndustrial;
        s.roll = isIndustrial ? 0f : 180f;
        s.shipId = id;
        s.speed = isIndustrial ? 2.5f : 4f;
        s.dropsPerPile = isIndustrial ? 18 : 10;
        s.maxTrashOnMap = 200;
        s.dropInterval = new Vector2(2.5f, 4.5f);
    }

    static void AddPlayerBody(GameObject player)
    {
        ModelSkin.DressPlayer(player);
        foreach (var rend in player.GetComponentsInChildren<Renderer>())
            if (rend.name != "CollectZone") return;

        var visual = Prim(PrimitiveType.Capsule, "RobotBody", Vector3.zero, Vector3.one, new Color(0.2f, 0.7f, 0.3f), player.transform);
        visual.transform.localPosition = Vector3.zero;
        Object.Destroy(visual.GetComponent<Collider>());
        var nose = Prim(PrimitiveType.Cube, "RobotFront", Vector3.zero, new Vector3(0.4f, 0.3f, 0.4f), Color.black, player.transform);
        nose.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
        Object.Destroy(nose.GetComponent<Collider>());
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

    class ShipSpawnRunner : MonoBehaviour { }
}
