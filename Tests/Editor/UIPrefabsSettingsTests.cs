using System;
using System.Linq;
using ActionFit.SOSingleton;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionFit.UIPrefabs.Editor.Tests
{
    public class UIPrefabsSettingsTests
    {
        private bool _createdSettings;
        private bool[] _folderExisted;

        private static readonly string[] SettingsFolders =
        {
            "Assets/_Data",
            "Assets/_Data/_UI Prefabs"
        };

        [TearDown]
        public void TearDown()
        {
            if (_createdSettings)
            {
                AssetDatabase.DeleteAsset(UIPrefabsSettingsUtility.DefaultAssetPath);
                _createdSettings = false;
            }

            if (_folderExisted != null)
            {
                for (int index = SettingsFolders.Length - 1; index >= 0; index--)
                {
                    if (!_folderExisted[index] && IsEmptyFolder(SettingsFolders[index]))
                        AssetDatabase.DeleteAsset(SettingsFolders[index]);
                }
            }

            AssetDatabase.Refresh();
            _folderExisted = null;
        }

        [Test]
        public void DefaultSettingsPathIsProjectOwned()
        {
            Assert.That(
                UIPrefabsSettingsUtility.DefaultAssetPath,
                Is.EqualTo("Assets/_Data/_UI Prefabs/UIPrefabsSO.asset"));
            Assert.That(UIPrefabsSettingsUtility.DefaultAssetPath, Does.Not.StartWith("Packages/"));
            Assert.That(UIPrefabsSettingsUtility.DefaultAssetPath, Does.Not.Contain("/Resources/"));

            var registration = (ActionFitSettingsAssetAttribute)Attribute.GetCustomAttribute(
                typeof(UIPrefabsSO),
                typeof(ActionFitSettingsAssetAttribute));
            Assert.That(registration, Is.Not.Null);
            Assert.That(registration.Lifetime, Is.EqualTo(ActionFitSettingsAssetLifetime.EditorOnly));
            Assert.That(
                registration.LegacyPaths,
                Does.Contain("Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset"));
        }

        [Test]
        public void LoadOrCreateReturnsStableCanonicalAsset()
        {
            _folderExisted = SettingsFolders.Select(AssetDatabase.IsValidFolder).ToArray();
            bool existed = AssetDatabase.LoadAssetAtPath<UIPrefabsSO>(
                UIPrefabsSettingsUtility.DefaultAssetPath) != null;
            UIPrefabsSO first = UIPrefabsSettingsUtility.LoadOrCreate();
            _createdSettings = !existed;
            UIPrefabsSO second = UIPrefabsSettingsUtility.LoadOrCreate();
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
            Assert.That(AssetDatabase.GetAssetPath(first), Is.EqualTo(UIPrefabsSettingsUtility.DefaultAssetPath));
        }

        private static bool IsEmptyFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) return false;

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { path }))
            {
                string childPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.Equals(childPath, path, StringComparison.Ordinal)) return false;
            }

            return true;
        }

        [Test]
        public void AllEntriesKeepsBaseOrderAndNormalizesCustomEntry()
        {
            UIPrefabsSO settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
            var prefab = new GameObject("NeutralCustomPrefab");
            try
            {
                settings.Image = prefab;
                settings.Text = prefab;
                settings.Button = prefab;
                settings.Input = prefab;
                settings.InputBtn = prefab;
                settings.Scroll = prefab;
                settings.Mask = prefab;
                settings.Mask2D = prefab;
                settings.Fill = prefab;
                settings.Custom = new[]
                {
                    null,
                    new UIPrefabsSO.Entry { Label = "", Category = "", Prefab = prefab }
                };

                UIPrefabsSO.Entry[] entries = settings.AllEntries().ToArray();
                Assert.That(entries.Take(9).Select(entry => entry.Label), Is.EqualTo(new[]
                {
                    "Image", "Text", "Button", "Input", "InputBtn", "Scroll", "Mask", "Mask2D", "Fill"
                }));
                Assert.That(entries.Take(9).All(entry => entry.Category == "Base"), Is.True);
                Assert.That(entries.Length, Is.EqualTo(10));
                Assert.That(entries[9].Label, Is.EqualTo(prefab.name));
                Assert.That(entries[9].Category, Is.EqualTo("Custom"));
            }
            finally
            {
                Object.DestroyImmediate(settings);
                Object.DestroyImmediate(prefab);
            }
        }
    }
}
