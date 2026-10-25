using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildMapInScene
{
    const float Width = 60f;

    [MenuItem("Tools/Map/Build Beach Town in open scene")]
    static void BeachTown() => Build("Maps/BeachTown/Meshy_AI_diorama_beach_town_ti_1024110804_texture", "Maps/BeachTown/BeachTown_Mat");

    [MenuItem("Tools/Map/Build Ocean Reef in open scene")]
    static void OceanReef() => Build("Maps/OceanReef/Meshy_AI_diorama_ocean_reef_ti_1024110834_texture", "Maps/OceanReef/OceanReef_Mat");

    static void Build(string model, string material)
    {
        var old = GameObject.Find("MapRoot");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var root = new GameObject("MapRoot");
        Undo.RegisterCreatedObjectUndo(root, "Build map");

        var go = MapModel.Place(model, root.transform, Width, material);
        if (go == null) { Object.DestroyImmediate(root); return; }

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log($"Map built under 'MapRoot' in scene '{root.scene.name}'. Save the scene (Ctrl+S).");
    }
}
