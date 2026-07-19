using ActionFit.SOSingleton.Editor;

/// <summary>UI Prefabs 설정 SO를 패키지 밖 프로젝트 Assets에서 생성·조회합니다.</summary>
public static class UIPrefabsSettingsUtility
{
    public const string MenuPath = "Tools/Package/UI Prefabs/Setting SO";
    public const string DefaultAssetPath = "Assets/_Data/_UI Prefabs/UIPrefabsSO.asset";

    /// <summary>기본 경로의 설정을 반환하고, 기존 호환 자산이 단 하나면 그 자산을 사용합니다.</summary>
    public static UIPrefabsSO LoadExisting()
    {
        return ActionFitSettingsAssetProvider.Resolve(typeof(UIPrefabsSO), false).Asset
            as UIPrefabsSO;
    }

    /// <summary>설정이 없으면 프로젝트 Assets의 기본 경로에 생성하고 반환합니다.</summary>
    public static UIPrefabsSO LoadOrCreate()
    {
        return ActionFitSettingsAssetProvider.GetOrCreate<UIPrefabsSO>();
    }
}
