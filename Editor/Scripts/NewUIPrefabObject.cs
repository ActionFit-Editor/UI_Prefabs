using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Hierarchy 우클릭 > GameObject > >>>UI_Prefab > Add UI Prefab... 에서 표준/커스텀 UI prefab을
/// 선택된 Canvas 하위에 인스턴스화합니다.
/// - 단일 [MenuItem] 진입점 → 클릭 시 UIPrefabsSO.AllEntries()를 GenericMenu로 동적 구성.
///   (Unity [MenuItem]은 컴파일 타임 정적이라 SO 배열로 메뉴 항목을 직접 생성할 수 없어 이 방식 사용.)
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
    // (데이터 기반인 Custom은 정적 메뉴로 못 만들어 아래 Add UI Prefab... 팝업으로 처리.)
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

    #region Editor Menu — Custom (데이터 기반 GenericMenu 팝업)

    // SO의 Custom Entry들을 GenericMenu로 띄운다. SO 편집은 다음 열람에 즉시 반영(재컴파일 불요).
    // (Base는 위 직접 [MenuItem]으로 제공하므로 팝업엔 Custom만 노출 — 중복 방지.)
    [MenuItem(ROOT, false, 20)]
    private static void OpenMenu()
    {
        var so = LoadSO();
        if (so == null)
        {
            UnityEngine.Debug.LogWarning(
                $"[NewUIPrefabObject] UIPrefabsSO not found. Open '{UIPrefabsSettingsUtility.MenuPath}' to create it.");
            return;
        }

        var menu = new GenericMenu();
        var seen = new HashSet<string>();
        foreach (var entry in so.AllEntries())
        {
            if (entry.Category == "Base") continue;    // Base는 직접 메뉴 항목으로 제공 → 팝업엔 Custom만
            var prefab = entry.Prefab;                 // 클로저 캡처용 per-iteration 지역변수
            if (prefab == null) continue;

            string path = string.IsNullOrEmpty(entry.Category) ? entry.Label : entry.Category + "/" + entry.Label;
            if (!seen.Add(path))                       // GenericMenu는 동일 경로를 무음 병합 → 중복 차단
            {
                UnityEngine.Debug.LogWarning($"[NewUIPrefabObject] 중복 메뉴 경로 '{path}' 무시됨 — Custom의 Label/Category를 고유하게 설정하세요");
                continue;
            }
            menu.AddItem(new GUIContent(path), false, () => Instantiate(prefab));
        }
        menu.ShowAsContext();
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
    [MenuItem(ROOT, true)]
    private static bool ValidateUnderCanvas()
        => Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<Canvas>();

    #endregion

    #region Generals

    // prefab을 Selection.activeTransform(Canvas 하위) 아래 인스턴스화. 클릭 시점에 부모/Canvas를 재확인.
    private static void Instantiate(GameObject prefab)
    {
        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning("[NewUIPrefabObject] prefab이 비어있음");
            return;
        }

        var parent = Selection.activeTransform;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            UnityEngine.Debug.LogWarning("[NewUIPrefabObject] Canvas 하위 GameObject를 선택한 상태에서 실행하세요");
            return;
        }

        Object instance = PrefabUtility.InstantiatePrefab(prefab, parent);
        if (instance == null)
        {
            UnityEngine.Debug.LogWarning("[NewUIPrefabObject] InstantiatePrefab 실패");
            return;
        }

        Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
        Selection.activeObject = instance;
    }

    #endregion
}
