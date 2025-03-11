using UnityEngine;
using UnityEditor;
using System.IO;
using EzBoost.EzSave;

/// <summary>
/// Editor menu items for EzSave settings
/// </summary>
public static class EzSaveSettingsEditorMenu
{
    private const string SettingsPath = "Assets/Runtime/EzBoost/EzSave/Resources";
    private const string SettingsAssetName = "EzSaveDefaultSetting.asset";
    private const string ResourcePath = "EzSaveDefaultSetting";
    
    [MenuItem("Tools/EzSave/Settings", false, 100)]
    public static void OpenSettings()
    {
        // Try to find the settings asset first
        var settings = Resources.Load<EzSaveDefaultSetting>(ResourcePath);
        
        // If settings don't exist, create them
        if (settings == null)
        {
            if (EditorUtility.DisplayDialog("EzSave Settings", 
                "Default settings asset doesn't exist yet. Do you want to create it?", 
                "Yes", "No"))
            {
                CreateSettingsAsset();
                settings = Resources.Load<EzSaveDefaultSetting>(ResourcePath);
            }
            else
            {
                return;
            }
        }
        
        // Open the settings asset in the inspector
        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }
    
    [MenuItem("Tools/EzSave/Create Default Settings Asset", false, 101)]
    public static void CreateSettingsAssetMenuItem()
    {
        CreateSettingsAsset();
    }
    
    private static void CreateSettingsAsset()
    {
        // Create the directory if it doesn't exist
        if (!Directory.Exists(SettingsPath))
        {
            Directory.CreateDirectory(SettingsPath);
            AssetDatabase.Refresh();
        }
        
        // Check if asset already exists
        string fullPath = Path.Combine(SettingsPath, SettingsAssetName);
        if (File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("EzSave Settings", 
                "Settings asset already exists at " + fullPath, 
                "OK");
            return;
        }
        
        // Create the settings asset
        var settings = ScriptableObject.CreateInstance<EzSaveDefaultSetting>();
        AssetDatabase.CreateAsset(settings, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("EzSave default settings created at " + fullPath);
        
        // Select the created asset
        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }
} 