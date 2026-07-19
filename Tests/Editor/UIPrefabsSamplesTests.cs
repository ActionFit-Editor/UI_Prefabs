using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ActionFit.UIPrefabs.Editor.Tests
{
    public class UIPrefabsSamplesTests
    {
        private const string SourceRoot = "Packages/com.actionfit.ui.prefabs/Samples~/Starter UI Prefabs";
        private const string SampleCatalog = "UIPrefabsSO.asset";
        private string _importedRoot;
        private bool _ownsImportedRoot;

        private static readonly IReadOnlyDictionary<string, Type> Prefabs = new Dictionary<string, Type>
        {
            ["Image"] = typeof(UI_Image),
            ["Text"] = typeof(UI_Text),
            ["Button"] = typeof(UI_Button),
            ["Input"] = typeof(UI_Input),
            ["InputBtn"] = typeof(UI_InputBtn),
            ["Scroll"] = typeof(UI_Scroll),
            ["Mask"] = typeof(UI_Mask),
            ["Mask2D"] = typeof(UI_Mask2D),
            ["Fill"] = typeof(UI_ImageSlice)
        };

        [SetUp]
        public void SetUp()
        {
            _importedRoot = $"Assets/__ActionFitUIPrefabsSampleTests_{Guid.NewGuid():N}";
            _ownsImportedRoot = false;
        }

        [TearDown]
        public void TearDown()
        {
            if (!_ownsImportedRoot) return;

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (AssetDatabase.IsValidFolder(_importedRoot)) AssetDatabase.DeleteAsset(_importedRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            _ownsImportedRoot = false;
        }

        [Test]
        public void ManifestExposesStarterSample()
        {
            string manifest = File.ReadAllText("Packages/com.actionfit.ui.prefabs/package.json");
            StringAssert.Contains("\"path\": \"Samples~/Starter UI Prefabs\"", manifest);
            Assert.That(Directory.Exists(SourceRoot), Is.True);
        }

        [Test]
        public void StarterPrefabsImportWithoutMissingScriptsOrProjectDependencies()
        {
            Assert.That(Directory.Exists(SourceRoot), Is.True);
            Assert.That(Directory.Exists(_importedRoot), Is.False, _importedRoot);
            _ownsImportedRoot = true;
            FileUtil.CopyFileOrDirectory(SourceRoot, _importedRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (KeyValuePair<string, Type> expected in Prefabs)
            {
                string path = $"{_importedRoot}/{expected.Key}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(prefab.GetComponentInChildren(expected.Value, true), Is.Not.Null, path);
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                {
                    Assert.That(
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject),
                        Is.Zero,
                        $"{path} ({child.name})");
                }
                AssertMigratedButtons(path, prefab);
                AssertNeutralDependencies(path);
            }

            var catalog = AssetDatabase.LoadAssetAtPath<UIPrefabsSO>($"{_importedRoot}/{SampleCatalog}");
            Assert.That(catalog, Is.Not.Null);
            AssertCatalogEntry(catalog.Image, "Image");
            AssertCatalogEntry(catalog.Text, "Text");
            AssertCatalogEntry(catalog.Button, "Button");
            AssertCatalogEntry(catalog.Input, "Input");
            AssertCatalogEntry(catalog.InputBtn, "InputBtn");
            AssertCatalogEntry(catalog.Scroll, "Scroll");
            AssertCatalogEntry(catalog.Mask, "Mask");
            AssertCatalogEntry(catalog.Mask2D, "Mask2D");
            AssertCatalogEntry(catalog.Fill, "Fill");
            AssertNeutralDependencies($"{_importedRoot}/{SampleCatalog}");
        }

        private void AssertCatalogEntry(GameObject prefab, string name)
        {
            Assert.That(prefab, Is.Not.Null, name);
            Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo($"{_importedRoot}/{name}.prefab"));
        }

        private static void AssertMigratedButtons(string path, GameObject prefab)
        {
            foreach (UI_Button button in prefab.GetComponentsInChildren<UI_Button>(true))
            {
                Assert.That(button.GetComponent<UnityEngine.UI.Button>(), Is.Null, $"{path} ({button.name})");
                Assert.That(button.GetComponent<UIButtonPressEffect>(), Is.Null, $"{path} ({button.name})");
            }
        }

        private void AssertNeutralDependencies(string assetPath)
        {
            foreach (string dependency in AssetDatabase.GetDependencies(assetPath, true))
            {
                bool allowed = dependency.StartsWith(_importedRoot, StringComparison.Ordinal)
                    || dependency.StartsWith("Packages/com.actionfit.ui.foundation/", StringComparison.Ordinal)
                    || dependency.StartsWith("Packages/com.actionfit.ui.prefabs/", StringComparison.Ordinal)
                    || dependency.StartsWith("Packages/com.unity.", StringComparison.Ordinal);
                Assert.That(allowed, Is.True, $"Project-specific sample dependency: {assetPath} -> {dependency}");
            }
        }
    }
}
