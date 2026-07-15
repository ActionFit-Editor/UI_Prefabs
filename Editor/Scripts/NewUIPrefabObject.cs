using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Hierarchy 우클릭 > GameObject > >>>UI_Prefab에서 표준 UI prefab을 바로 생성하거나
/// Add UI Prefab... 창을 열어 custom prefab을 생성, 등록, 인스턴스화합니다.
/// - PrefabUtility.InstantiatePrefab 사용 → 원본 prefab 링크 유지 (Override 가능)
/// - Selection.activeTransform이 부모가 됨, Canvas 하위에서만 동작
/// - Undo 지원 (Ctrl+Z)
/// </summary>
public static class NewUIPrefabObject
{
    #region Properties

    private const string ROOT = "GameObject/>>>UI_Prefab/Add UI Prefab...";

    private static UIPrefabsSO LoadSO() => UIPrefabsSettingsUtility.LoadExisting();

    #endregion

    #region Editor Menu — Base (직접 생성 버튼)

    // Base prefab은 SO의 고정 필드라 정적 [MenuItem]으로 "바로 생성" 버튼을 직접 제공한다.
    // 데이터 기반 Custom은 UIPrefabsCustomMenuGenerator가 project-owned 메뉴 code로 동기화한다.
    [MenuItem("GameObject/>>>UI_Prefab/Image", false, 1)]    private static void CreateImage()    => CreateBase(p => p.Image);
    [MenuItem("GameObject/>>>UI_Prefab/Text", false, 1)]     private static void CreateText()     => CreateBase(p => p.Text);
    [MenuItem("GameObject/>>>UI_Prefab/Button", false, 1)]   private static void CreateButton()   => CreateBase(p => p.Button);
    [MenuItem("GameObject/>>>UI_Prefab/Input", false, 1)]    private static void CreateInput()    => CreateBase(p => p.Input);
    [MenuItem("GameObject/>>>UI_Prefab/InputBtn", false, 1)] private static void CreateInputBtn() => CreateBase(p => p.InputBtn);
    [MenuItem("GameObject/>>>UI_Prefab/Scroll", false, 1)]   private static void CreateScroll()   => CreateBase(p => p.Scroll);
    [MenuItem("GameObject/>>>UI_Prefab/Mask", false, 1)]     private static void CreateMask()     => CreateBase(p => p.Mask);
    [MenuItem("GameObject/>>>UI_Prefab/Mask2D", false, 1)]     private static void CreateMask2D()     => CreateBase(p => p.Mask2D);
    [MenuItem("GameObject/>>>UI_Prefab/Fill", false, 1)]     private static void CreateFill()     => CreateBase(p => p.Fill);

    private static void CreateBase(System.Func<UIPrefabsSO, GameObject> selector)
    {
        var so = LoadSO();
        if (so == null)
        {
            UnityEngine.Debug.LogWarning(
                $"[NewUIPrefabObject] UIPrefabsSO not found. Open '{UIPrefabsSettingsUtility.MenuPath}' to create it.");
            return;
        }
        Instantiate(selector(so));
    }

    #endregion

    #region Editor Menu — Custom

    // Custom prefab 생성/등록/사용 창은 등록 항목이나 Canvas 선택이 없어도 항상 열립니다.
    // 창 내부에서 유효한 대상과 source를 안내해 메뉴 클릭이 무반응처럼 보이지 않게 합니다.
    [MenuItem(ROOT, false, 20)]
    private static void OpenMenu()
    {
        UIPrefabAddWindow.Open(Selection.activeTransform);
    }

    [MenuItem("GameObject/>>>UI_Prefab/Image", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Text", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Button", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Input", true)]
    [MenuItem("GameObject/>>>UI_Prefab/InputBtn", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Scroll", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Mask", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Mask2D", true)]
    [MenuItem("GameObject/>>>UI_Prefab/Fill", true)]
    private static bool ValidateUnderCanvas()
        => CanInstantiateUnderSelection();

    #endregion

    #region Generals

    // prefab을 Selection.activeTransform(Canvas 하위) 아래 인스턴스화. 클릭 시점에 부모/Canvas를 재확인.
    private static void Instantiate(GameObject prefab)
    {
        if (!TryInstantiate(prefab, Selection.activeTransform, out _, out string error))
        {
            UnityEngine.Debug.LogWarning($"[NewUIPrefabObject] {error}");
        }
    }

    /// <summary>등록된 prefab을 Canvas 하위에 연결 상태로 생성하고 Undo에 등록합니다.</summary>
    public static bool TryInstantiate(
        GameObject prefab,
        Transform parent,
        out GameObject instance,
        out string error)
    {
        instance = null;
        error = null;

        if (prefab == null)
        {
            error = "Prefab is empty.";
            return false;
        }

        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            error = "Select a GameObject under a Canvas.";
            return false;
        }

        Object created = PrefabUtility.InstantiatePrefab(prefab, parent);
        instance = created as GameObject;
        if (instance == null)
        {
            error = "PrefabUtility.InstantiatePrefab failed.";
            return false;
        }

        Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
        Selection.activeObject = instance;
        return true;
    }

    /// <summary>generated Custom 메뉴에서 prefab GUID를 조회해 현재 선택 아래에 생성합니다.</summary>
    public static void InstantiateRegisteredPrefabByGuid(string prefabGuid)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning(
                $"[NewUIPrefabObject] Registered prefab not found for GUID '{prefabGuid}'. Refresh UIPrefabsSO.Custom.");
            return;
        }

        Instantiate(prefab);
    }

    /// <summary>현재 선택이 Canvas 자신 또는 하위 오브젝트인지 반환합니다.</summary>
    public static bool CanInstantiateUnderSelection()
        => Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<Canvas>() != null;

    /// <summary>Custom 메뉴 경로를 정규화하고 기존 Base/Custom 경로와의 중복을 검사합니다.</summary>
    public static bool TryValidateCustomEntry(
        UIPrefabsSO settings,
        GameObject prefab,
        string label,
        string category,
        out string normalizedLabel,
        out string normalizedCategory,
        out string error)
    {
        normalizedLabel = string.IsNullOrWhiteSpace(label)
            ? prefab != null ? prefab.name : string.Empty
            : label.Trim();
        normalizedCategory = string.IsNullOrWhiteSpace(category) ? "Custom" : category.Trim();
        error = null;

        if (settings == null)
        {
            error = "UIPrefabsSO is not available.";
            return false;
        }

        if (prefab == null)
        {
            error = "Choose a source UI object or prefab asset.";
            return false;
        }

        if (string.IsNullOrEmpty(normalizedLabel))
        {
            error = "Label cannot be empty.";
            return false;
        }

        string candidatePath = BuildMenuPath(normalizedLabel, normalizedCategory);
        foreach (UIPrefabsSO.Entry entry in settings.AllEntries())
        {
            if (entry?.Prefab == null) continue;
            if (string.Equals(
                    candidatePath,
                    BuildMenuPath(entry.Label, entry.Category),
                    StringComparison.Ordinal))
            {
                error = $"Menu path '{candidatePath}' is already registered.";
                return false;
            }
        }

        return true;
    }

    /// <summary>저장된 prefab asset을 기존 Custom 배열 끝에 추가하고 설정 asset을 저장합니다.</summary>
    public static bool TryRegisterCustomPrefab(
        UIPrefabsSO settings,
        GameObject prefab,
        string label,
        string category,
        out string error)
    {
        if (!TryValidateCustomEntry(
                settings,
                prefab,
                label,
                category,
                out string normalizedLabel,
                out string normalizedCategory,
                out error))
        {
            return false;
        }

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(prefabPath) ||
            !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
            !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            error = "The source must be a saved prefab asset.";
            return false;
        }

        Undo.RecordObject(settings, "Register UI Prefab");
        UIPrefabsSO.Entry[] custom = settings.Custom ?? Array.Empty<UIPrefabsSO.Entry>();
        Array.Resize(ref custom, custom.Length + 1);
        custom[custom.Length - 1] = new UIPrefabsSO.Entry
        {
            Label = normalizedLabel,
            Category = normalizedCategory,
            Prefab = prefab,
        };
        settings.Custom = custom;
        EditorUtility.SetDirty(settings);
        if (EditorUtility.IsPersistent(settings))
        {
            AssetDatabase.SaveAssetIfDirty(settings);
            UIPrefabsCustomMenuGenerator.ScheduleSync();
        }
        return true;
    }

    private static string BuildMenuPath(string label, string category)
        => string.IsNullOrEmpty(category) ? label : category + "/" + label;

    #endregion
}
