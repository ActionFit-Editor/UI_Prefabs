using ActionFit.SOSingleton;
using UnityEngine;

/// <summary>
/// Hierarchy 우클릭 > GameObject > >>>UI_Prefab 메뉴에서 인스턴스화할 prefab들을 보관하는 에디터 전용 SO.
/// 기본 자산 위치: Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset
/// NewUIPrefabObject(Editor)가 AssetDatabase로 로드하여 PrefabUtility.InstantiatePrefab 호출.
///
/// - Base: 패키지 기본 제공 UI 프리팹(이 프로젝트 기준으로 사전 배선된 명명 필드).
/// - Custom: 사용자가 직접 끌어다 등록하는 가변 프리팹 목록(다른 프로젝트에서 자유 확장).
/// - AllEntries(): Base + Custom 을 하나의 Entry 스트림으로 노출.
/// - Custom 직접 생성 메뉴는 project-owned generated Editor code로 동기화됨.
/// </summary>
[CreateAssetMenu(menuName = "SO/UI/UIPrefabsSO", fileName = "UIPrefabsSO")]
[ActionFitSettingsAsset(
    "UI Prefabs",
    ActionFitSettingsAssetLifetime.EditorOnly,
    LegacyPaths = new string[]
    {
        "Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset"
    })]
public class UIPrefabsSO : ScriptableObject
{
#if UNITY_EDITOR
    /// <summary>메뉴에 표시할 단일 프리팹 항목. Base/Custom 공통 타입.</summary>
    [System.Serializable]
    public class Entry
    {
        public string Label;     // 메뉴 표시명 (비우면 Prefab.name)
        public string Category;  // 서브메뉴 그룹명 (비우면 "Custom")
        public GameObject Prefab;
    }

    [Header("Base")]
    public GameObject Image;
    public GameObject Text;
    public GameObject Button;
    public GameObject Input;
    public GameObject InputBtn;
    public GameObject Scroll;
    public GameObject Mask;
    public GameObject Mask2D;
    public GameObject Fill;

    [Header("Custom (drag your prefabs here)")]
    public Entry[] Custom;

    /// <summary>Base(고정) + Custom(가변)을 하나의 Entry 스트림으로 순회한다. null prefab은 건너뜀.</summary>
    public System.Collections.Generic.IEnumerable<Entry> AllEntries()
    {
        yield return new Entry { Label = "Image",    Category = "Base", Prefab = Image };
        yield return new Entry { Label = "Text",     Category = "Base", Prefab = Text };
        yield return new Entry { Label = "Button",   Category = "Base", Prefab = Button };
        yield return new Entry { Label = "Input",    Category = "Base", Prefab = Input };
        yield return new Entry { Label = "InputBtn", Category = "Base", Prefab = InputBtn };
        yield return new Entry { Label = "Scroll",   Category = "Base", Prefab = Scroll };
        yield return new Entry { Label = "Mask",     Category = "Base", Prefab = Mask };
        yield return new Entry { Label = "Mask2D",   Category = "Base", Prefab = Mask2D };
        yield return new Entry { Label = "Fill",     Category = "Base", Prefab = Fill };

        if (Custom == null) yield break;
        foreach (var e in Custom)
        {
            if (e == null || e.Prefab == null) continue;
            yield return new Entry
            {
                Label    = string.IsNullOrEmpty(e.Label)    ? e.Prefab.name : e.Label,
                Category = string.IsNullOrEmpty(e.Category) ? "Custom"       : e.Category,
                Prefab   = e.Prefab,
            };
        }
    }
#endif
}
