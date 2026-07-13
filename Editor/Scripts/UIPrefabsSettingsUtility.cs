using System;
using UnityEditor;
using UnityEngine;

/// <summary>UI Prefabs 설정 SO를 패키지 밖 프로젝트 Assets에서 생성·조회합니다.</summary>
public static class UIPrefabsSettingsUtility
{
    public const string MenuPath = "Tools/Package/UI Prefabs/Setting SO";
    public const string DefaultAssetPath = "Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset";

    /// <summary>기본 경로의 설정을 반환하고, 기존 호환 자산이 단 하나면 그 자산을 사용합니다.</summary>
    public static UIPrefabsSO LoadExisting()
    {
        UIPrefabsSO settings = AssetDatabase.LoadAssetAtPath<UIPrefabsSO>(DefaultAssetPath);
        if (settings != null) return settings;

        string[] guids = AssetDatabase.FindAssets("t:UIPrefabsSO");
        if (guids.Length == 1)
        {
            return AssetDatabase.LoadAssetAtPath<UIPrefabsSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (guids.Length > 1)
        {
            Debug.LogWarning(
                $"[UIPrefabsSettingsUtility] Multiple UIPrefabsSO assets found. Move the intended asset to '{DefaultAssetPath}'.");
        }

        return null;
    }

    /// <summary>설정이 없으면 프로젝트 Assets의 기본 경로에 생성하고 반환합니다.</summary>
    public static UIPrefabsSO LoadOrCreate()
    {
        UIPrefabsSO settings = LoadExisting();
        if (settings != null) return settings;

        EnsureFolder("Assets/Editor");
        EnsureFolder("Assets/Editor/ActionFit");
        EnsureFolder("Assets/Editor/ActionFit/UI Prefabs");

        settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
        AssetDatabase.CreateAsset(settings, DefaultAssetPath);
        AssetDatabase.SaveAssets();
        return settings;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        int separator = path.LastIndexOf('/');
        string parent = path.Substring(0, separator);
        string name = path.Substring(separator + 1);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
