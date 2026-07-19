# UI Prefabs (`com.actionfit.ui.prefabs`)

`UI Foundation`을 사용하는 프로젝트에 에디터 제작 메뉴, 프로젝트 소유 설정, 중립 시작 샘플을 더하는 선택 패키지입니다. 런타임 UI 타입은 `com.actionfit.ui.foundation`이 소유하며 이 패키지는 단방향으로만 의존합니다.

## 설정 SO 수명주기

`UIPrefabsSO`는 프로젝트 소유 `Assets/_Data/_UI Prefabs/UIPrefabsSO.asset`을 canonical 경로로 사용합니다. 기존 `Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset`도 legacy 후보로 재사용하며 prefab reference, GUID, 직렬화 값은 이동하거나 다시 쓰지 않습니다. 중복 설정은 자동 선택·생성을 막습니다.

## 설치

Unity Package Manager의 Git dependency로 두 패키지를 설치합니다.

```json
{
  "dependencies": {
    "com.actionfit.ui.foundation": "https://github.com/ActionFit-Editor/UI_Foundation.git#2.0.0",
    "com.actionfit.ui.prefabs": "https://github.com/ActionFit-Editor/UI_Prefabs.git#2.0.1"
  }
}
```

`com.actionfit.ui.prefabs`의 `package.json`도 Foundation `2.0.0` 의존성을 선언합니다. 위 URL은 Public 저장소 배포용 계약이며 실제 원격 저장소와 태그 발행은 Custom Package Manager에서 별도로 수행합니다.

## 사용

- `Tools > Package > UI Prefabs > Setting SO`: 프로젝트의 canonical `Assets/_Data/_UI Prefabs/UIPrefabsSO.asset` 또는 재사용 가능한 기존 legacy 설정을 엽니다.
- `GameObject > >>>UI_Prefab`: 선택한 Canvas 아래에 설정된 prefab을 연결 상태로 생성합니다.
- `GameObject > >>>UI_Prefab > Add UI Prefab...`: 등록된 Custom prefab을 생성하거나, Canvas 하위 오브젝트를 새 prefab asset으로 저장·등록하고 기존 prefab asset을 바로 등록하는 창을 엽니다. Custom 항목이나 Canvas 선택이 없어도 창은 열리고 필요한 입력을 안내합니다.
- `GameObject > >>>UI_Prefab > <Category> > <Label>`: `UIPrefabsSO.Custom` 항목을 창을 거치지 않고 바로 생성합니다. 등록, Inspector 수정, asset 재가져오기, Editor 재로드 시 `Assets/Editor/ActionFitUIPrefabsGeneratedMenuItems.cs`가 자동 동기화되고 컴파일 후 메뉴에 반영됩니다.
- `Tools > Package > UI Prefabs > README`: 이 문서를 엽니다.

설정과 generated Custom 메뉴 코드는 다운로드 패키지 안에 저장하지 않습니다. 기본 경로의 설정이 없고 프로젝트 전체에 `UIPrefabsSO`가 하나만 있으면 기존 자산을 호환 경로로 사용하며, 여러 개면 기본 경로로 옮기도록 경고합니다. generated 파일은 파생 산출물이므로 직접 수정하지 않습니다.

## Starter UI Prefabs 샘플

Package Manager의 Samples에서 `Starter UI Prefabs`를 import하면 Image, Text, Button, Input, InputBtn, Scroll, Mask, Mask2D, Fill 예제와 샘플 catalog를 받을 수 있습니다.

- 샘플은 Unity UI/TMP와 `UI Foundation`만 사용합니다.
- Button과 InputBtn 샘플의 `UI_Button`은 native `UnityEngine.UI.Button`이나 `UIButtonPressEffect`를 함께 두지 않고 Foundation 2.0의 자체 포인터/press 동작을 사용합니다.
- 냥카페 폰트, 이미지, `Assets/_Project`, 게임 전용 서비스에는 의존하지 않습니다.
- 실제 프로젝트의 색상, 폰트, 이미지, 레이아웃은 import 후 교체합니다. TMP 자산이 없는 새 프로젝트에서는 **Window > TextMeshPro > Import TMP Essential Resources**를 먼저 실행합니다.

냥카페에 이미 있던 9개 example prefab과 Cat 전용 art/font는 프로젝트 `Assets`에 그대로 남으며 이 패키지에 포함되지 않습니다.

## 직렬화 호환

`UIPrefabsSO`, `NewUIPrefabObject`의 global 타입명과 기존 script GUID를 보존합니다. 기존 냥카페 설정 자산도 GUID를 유지한 채 `Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset`으로 이동합니다. 기존 필드 이름이나 타입을 바꿀 때는 소비 프로젝트의 prefab/asset 마이그레이션이 필요합니다.

## 어셈블리와 테스트

- Editor: `com.actionfit.ui.prefabs.Editor`
- Editor tests: `com.actionfit.ui.prefabs.Editor.Tests`
- Runtime assembly는 없으며 런타임 타입은 Foundation에서 제공합니다.

테스트는 설정의 프로젝트 소유 경로, script identity, sample 구성, missing script 여부와 샘플 `UI_Button`의 legacy component 부재를 검증합니다.

## Agent Skill 안내

schema v2 `Skills~/manifest.json`이 Codex와 Claude에 다음 read-only 스킬을 제공합니다.

- `$ui-prefabs-help`: 설정 조회, Foundation 의존성, 샘플과 생성 메뉴의 안전 경계를 설명합니다.
- `$ui-prefabs-audit`: 기존 `UIPrefabsSO`, prefab 참조, Missing Script, 중복 메뉴 경로와 샘플 중립성을 점검합니다.

audit은 `UIPrefabsSO` 생성·이동, 샘플 import, prefab 인스턴스 생성, scene/prefab 저장과 reserialize을 실행하지 않습니다.

## 배포

이 패키지의 repository visibility 메타데이터는 `Public`입니다. 원격 저장소 생성, 태그, 카탈로그 등록과 실제 배포는 자동으로 수행되지 않으며 Custom Package Manager의 수동 publish 절차를 사용합니다.
