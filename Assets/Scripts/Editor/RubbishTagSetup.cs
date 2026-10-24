using System.Linq;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
static class RubbishTagSetup
{
    static RubbishTagSetup() => EditorApplication.delayCall += Ensure;

    static void Ensure()
    {
        if (!InternalEditorUtility.tags.Contains(Trash.Tag)) InternalEditorUtility.AddTag(Trash.Tag);
    }
}
