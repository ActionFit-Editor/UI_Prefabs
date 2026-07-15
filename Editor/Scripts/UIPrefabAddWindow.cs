using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom UI prefab을 새 asset으로 저장하거나 기존 prefab asset을 UIPrefabsSO에 등록하고,
/// 등록 항목을 선택된 Canvas 하위에 즉시 생성하는 authoring 창입니다.
/// </summary>
public sealed class UIPrefabAddWindow : EditorWindow
{
    #region Fields

    private UIPrefabsSO _settings;
    private Transform _targetParent;
    private GameObject _source;
    private string _label = string.Empty;
    private string _category = "Custom";
    private Vector2 _registeredScroll;
    private string _message;
    private MessageType _messageType = MessageType.Info;

    #endregion

    #region Public Methods

    /// <summary>현재 선택을 초기 target/source로 사용해 Custom UI prefab authoring 창을 엽니다.</summary>
    public static void Open(Transform targetParent)
    {
        UIPrefabAddWindow window = GetWindow<UIPrefabAddWindow>(true, "Add UI Prefab", true);
        window.minSize = new Vector2(440f, 430f);
        window._targetParent = targetParent;
        window._source = Selection.activeGameObject;
        window.ApplySourceDefaults();
        window.Show();
        window.Focus();
    }

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        _settings = UIPrefabsSettingsUtility.LoadExisting();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Add UI Prefab", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Create a custom UI prefab from a Canvas object, or register an existing prefab asset. " +
            "Registered prefabs can be instantiated immediately below.",
            MessageType.Info);

        DrawSettings();
        EditorGUILayout.Space(8f);
        DrawTargetParent();
        EditorGUILayout.Space(8f);
        DrawRegisteredPrefabs();
        EditorGUILayout.Space(8f);
        DrawRegistration();

        if (!string.IsNullOrEmpty(_message))
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(_message, _messageType);
        }
    }

    #endregion

    #region Private Methods

    private void DrawSettings()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Settings", _settings, typeof(UIPrefabsSO), false);
            }
            if (GUILayout.Button("Open", GUILayout.Width(64f)) && _settings != null)
            {
                Selection.activeObject = _settings;
                EditorGUIUtility.PingObject(_settings);
            }
        }

        if (_settings == null)
        {
            string[] settingsGuids = AssetDatabase.FindAssets("t:UIPrefabsSO");
            if (settingsGuids.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No UIPrefabsSO exists. Create the project-owned settings asset to continue.",
                    MessageType.Warning);
                if (GUILayout.Button("Create Settings"))
                {
                    _settings = UIPrefabsSettingsUtility.LoadOrCreate();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Multiple UIPrefabsSO assets were found. Move the intended asset to " +
                    $"'{UIPrefabsSettingsUtility.DefaultAssetPath}'.",
                    MessageType.Error);
            }
        }
    }

    private void DrawTargetParent()
    {
        _targetParent = (Transform)EditorGUILayout.ObjectField(
            "Target Parent",
            _targetParent,
            typeof(Transform),
            true);

        if (!IsUnderCanvas(_targetParent))
        {
            EditorGUILayout.HelpBox(
                "Choose a target GameObject under a Canvas before creating a registered prefab instance.",
                MessageType.Warning);
        }
    }

    private void DrawRegisteredPrefabs()
    {
        EditorGUILayout.LabelField("Registered Custom Prefabs", EditorStyles.boldLabel);
        UIPrefabsSO.Entry[] custom = _settings != null ? _settings.Custom : null;
        if (custom == null || custom.Length == 0 || !Array.Exists(custom, entry => entry?.Prefab != null))
        {
            EditorGUILayout.HelpBox(
                "No custom UI prefabs are registered yet. Use the section below to add one.",
                MessageType.Info);
            return;
        }

        _registeredScroll = EditorGUILayout.BeginScrollView(
            _registeredScroll,
            GUILayout.MinHeight(70f),
            GUILayout.MaxHeight(150f));
        foreach (UIPrefabsSO.Entry entry in custom)
        {
            if (entry?.Prefab == null) continue;
            using (new EditorGUILayout.HorizontalScope())
            {
                string label = string.IsNullOrWhiteSpace(entry.Label) ? entry.Prefab.name : entry.Label;
                string category = string.IsNullOrWhiteSpace(entry.Category) ? "Custom" : entry.Category;
                EditorGUILayout.LabelField(category + "/" + label, GUILayout.MinWidth(150f));
                EditorGUILayout.ObjectField(entry.Prefab, typeof(GameObject), false);

                using (new EditorGUI.DisabledScope(!IsUnderCanvas(_targetParent)))
                {
                    if (GUILayout.Button("Create", GUILayout.Width(64f)))
                    {
                        if (NewUIPrefabObject.TryInstantiate(
                                entry.Prefab,
                                _targetParent,
                                out GameObject instance,
                                out string error))
                        {
                            SetMessage($"Created '{instance.name}' under '{_targetParent.name}'.", MessageType.Info);
                        }
                        else
                        {
                            SetMessage(error, MessageType.Error);
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawRegistration()
    {
        EditorGUILayout.LabelField("Register New UI Prefab", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _source = (GameObject)EditorGUILayout.ObjectField(
            "Source",
            _source,
            typeof(GameObject),
            true);
        if (EditorGUI.EndChangeCheck()) ApplySourceDefaults();

        _label = EditorGUILayout.TextField("Label", _label);
        _category = EditorGUILayout.TextField("Category", _category);

        bool isPrefabAsset = IsPrefabAsset(_source);
        string buttonLabel = isPrefabAsset ? "Register Prefab" : "Create Prefab & Register";
        using (new EditorGUI.DisabledScope(_settings == null || _source == null))
        {
            if (GUILayout.Button(buttonLabel, GUILayout.Height(28f))) RegisterSource(isPrefabAsset);
        }
    }

    private void RegisterSource(bool isPrefabAsset)
    {
        if (!NewUIPrefabObject.TryValidateCustomEntry(
                _settings,
                _source,
                _label,
                _category,
                out _,
                out _,
                out string validationError))
        {
            SetMessage(validationError, MessageType.Error);
            return;
        }

        GameObject prefab = _source;
        if (!isPrefabAsset)
        {
            if (!IsUnderCanvas(_source != null ? _source.transform : null))
            {
                SetMessage("A scene source must be a GameObject under a Canvas.", MessageType.Error);
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Create UI Prefab",
                _source.name,
                "prefab",
                "Choose where to save the new UI prefab asset.");
            if (string.IsNullOrEmpty(path))
            {
                SetMessage("Prefab creation was canceled. No settings were changed.", MessageType.Info);
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                SetMessage("Choose a new prefab path. Existing assets are not overwritten by this tool.", MessageType.Error);
                return;
            }

            prefab = PrefabUtility.SaveAsPrefabAsset(_source, path, out bool success);
            if (!success || prefab == null)
            {
                SetMessage("Failed to create the prefab asset. UIPrefabsSO was not changed.", MessageType.Error);
                return;
            }
        }

        if (!NewUIPrefabObject.TryRegisterCustomPrefab(
                _settings,
                prefab,
                _label,
                _category,
                out string error))
        {
            string suffix = isPrefabAsset
                ? string.Empty
                : " The prefab asset was created, but it was not registered.";
            SetMessage(error + suffix, MessageType.Error);
            return;
        }

        _source = prefab;
        _label = prefab.name;
        SetMessage($"Registered '{prefab.name}'. It is ready to create from the list above.", MessageType.Info);
        Repaint();
    }

    private void ApplySourceDefaults()
    {
        if (_source == null) return;
        _label = _source.name;
        if (string.IsNullOrWhiteSpace(_category)) _category = "Custom";
    }

    private void SetMessage(string message, MessageType type)
    {
        _message = message;
        _messageType = type;
    }

    private static bool IsUnderCanvas(Transform target)
        => target != null && target.GetComponentInParent<Canvas>() != null;

    private static bool IsPrefabAsset(GameObject gameObject)
    {
        if (gameObject == null) return false;
        string path = AssetDatabase.GetAssetPath(gameObject);
        return !string.IsNullOrEmpty(path)
            && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
            && PrefabUtility.IsPartOfPrefabAsset(gameObject);
    }

    #endregion
}
