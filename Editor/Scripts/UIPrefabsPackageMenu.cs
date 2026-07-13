#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class UIPrefabsPackageMenu
{
    private const string MenuRoot = "Tools/Package/UI Prefabs/";
    private const string ReadmePath = "Packages/com.actionfit.ui.prefabs/README.md";
    private const int SettingsPriority = 620;
    private const int ReadmePriority = 621;

    [MenuItem(MenuRoot + "Setting SO", false, SettingsPriority)]
    private static void SelectSettings()
    {
        UIPrefabsSO settings = UIPrefabsSettingsUtility.LoadOrCreate();
        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }

    [MenuItem(MenuRoot + "README", false, ReadmePriority)]
    private static void OpenReadme()
    {
        var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(ReadmePath);
        if (readme == null)
        {
            EditorUtility.DisplayDialog("Package README", $"README was not found.\n{ReadmePath}", "OK");
            return;
        }

        Selection.activeObject = readme;
        AssetDatabase.OpenAsset(readme);
    }
}
#endif
