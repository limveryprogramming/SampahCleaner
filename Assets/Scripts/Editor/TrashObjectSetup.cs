using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

public static class TrashObjectSetup
{
    [MenuItem("Tools/Create Trash Object")]
    static void Create()
    {
        if (!InternalEditorUtility.tags.Contains(Trash.Tag)) InternalEditorUtility.AddTag(Trash.Tag);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Trash";
        Undo.RegisterCreatedObjectUndo(go, "Create Trash Object");
        go.tag = Trash.Tag;
        go.transform.position = new Vector3(0f, 0.2f, 3f);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = new Material(rend.sharedMaterial) { color = new Color(0.45f, 0.3f, 0.15f) };

        go.GetComponent<Collider>().isTrigger = true;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        go.AddComponent<Trash>();

        Selection.activeObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Trash object created. Drag it from the Hierarchy into Assets/Resources/ to make the prefab named 'Trash', then delete it from the scene.");
    }
}
