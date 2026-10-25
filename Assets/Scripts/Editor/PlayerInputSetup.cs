using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerInputSetup
{
    const string AssetPath = "Assets/Scripts/Player/PlayerInputActions.inputactions";

    [MenuItem("Tools/Setup Player Input")]
    public static void Setup()
    {
        var asset = InputActionAsset.FromJson(File.ReadAllText(AssetPath));
        var map = asset.FindActionMap("Player");
        if (map == null)
        {
            Debug.LogError("PlayerInputActions has no 'Player' action map.");
            return;
        }

        bool changed = false;
        foreach (var required in new[] { "MoveAction", "OpenShop", "OpenCamera" })
            if (map.FindAction(required) == null)
                Debug.LogWarning($"Action '{required}' is missing from the Player map. Add it in the Input Actions editor.");

        if (map.FindAction("TakePhoto") == null)
        {
            var action = map.AddAction("TakePhoto", InputActionType.Button);
            action.AddBinding("<Mouse>/leftButton");
            changed = true;
            Debug.Log("Added TakePhoto action (Left Mouse Button).");
        }

        if (changed) File.WriteAllText(AssetPath, asset.ToJson());

        string metaPath = AssetPath + ".meta";
        string meta = File.ReadAllText(metaPath);
        if (!meta.Contains("generateWrapperCode: 1") || !meta.Contains("wrapperClassName: PlayerInputActions"))
        {
            meta = System.Text.RegularExpressions.Regex.Replace(meta, @"generateWrapperCode: \d", "generateWrapperCode: 1");
            meta = System.Text.RegularExpressions.Regex.Replace(meta, @"wrapperClassName:.*", "wrapperClassName: PlayerInputActions");
            File.WriteAllText(metaPath, meta);
            changed = true;
            Debug.Log("Enabled C# class generation (PlayerInputActions).");
        }

        if (changed) AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("Player input setup is up to date.");
    }
}
