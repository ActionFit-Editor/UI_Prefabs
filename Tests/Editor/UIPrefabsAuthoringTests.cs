using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionFit.UIPrefabs.Editor.Tests
{
    public class UIPrefabsAuthoringTests
    {
        private string _tempFolder;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _tempFolder = $"Assets/__ActionFitUIPrefabsAuthoringTests_{Guid.NewGuid():N}";
            AssetDatabase.CreateFolder("Assets", _tempFolder.Substring("Assets/".Length));

            var source = new GameObject("CustomButton", typeof(RectTransform));
            try
            {
                _prefab = PrefabUtility.SaveAsPrefabAsset(source, $"{_tempFolder}/CustomButton.prefab");
            }
            finally
            {
                Object.DestroyImmediate(source);
            }

            Undo.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();
            if (!string.IsNullOrEmpty(_tempFolder)) AssetDatabase.DeleteAsset(_tempFolder);
            AssetDatabase.Refresh();
        }

        [Test]
        public void RegisterCustomPrefabPreservesExistingEntriesAndNormalizesValues()
        {
            UIPrefabsSO settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
            try
            {
                settings.Custom = new[]
                {
                    new UIPrefabsSO.Entry
                    {
                        Label = "Existing",
                        Category = "Custom",
                        Prefab = _prefab,
                    }
                };

                bool registered = NewUIPrefabObject.TryRegisterCustomPrefab(
                    settings,
                    _prefab,
                    "  Added  ",
                    "  Event  ",
                    out string error);

                Assert.That(registered, Is.True, error);
                Assert.That(settings.Custom, Has.Length.EqualTo(2));
                Assert.That(settings.Custom[0].Label, Is.EqualTo("Existing"));
                Assert.That(settings.Custom[1].Label, Is.EqualTo("Added"));
                Assert.That(settings.Custom[1].Category, Is.EqualTo("Event"));
                Assert.That(settings.Custom[1].Prefab, Is.SameAs(_prefab));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void RegisterCustomPrefabRejectsDuplicateMenuPathWithoutMutation()
        {
            UIPrefabsSO settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
            try
            {
                settings.Custom = new[]
                {
                    new UIPrefabsSO.Entry
                    {
                        Label = "Button",
                        Category = "Event",
                        Prefab = _prefab,
                    }
                };

                bool registered = NewUIPrefabObject.TryRegisterCustomPrefab(
                    settings,
                    _prefab,
                    "Button",
                    "Event",
                    out string error);

                Assert.That(registered, Is.False);
                Assert.That(error, Does.Contain("Event/Button"));
                Assert.That(settings.Custom, Has.Length.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void InstantiateCreatesLinkedPrefabUnderCanvasAndSupportsUndo()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var parentObject = new GameObject("Target", typeof(RectTransform));
            parentObject.transform.SetParent(canvasObject.transform, false);
            try
            {
                bool created = NewUIPrefabObject.TryInstantiate(
                    _prefab,
                    parentObject.transform,
                    out GameObject instance,
                    out string error);

                Assert.That(created, Is.True, error);
                Assert.That(instance.transform.parent, Is.SameAs(parentObject.transform));
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(instance), Is.SameAs(_prefab));

                Undo.PerformUndo();
                Assert.That(instance == null, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(canvasObject);
            }
        }

        [Test]
        public void GeneratedSourceCreatesDirectCustomMenuCommandsByPrefabGuid()
        {
            UIPrefabsSO settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
            try
            {
                settings.Custom = new[]
                {
                    new UIPrefabsSO.Entry
                    {
                        Label = "Reward",
                        Category = "Custom",
                        Prefab = _prefab,
                    },
                    new UIPrefabsSO.Entry
                    {
                        Label = "GridReward",
                        Category = "Event",
                        Prefab = _prefab,
                    }
                };

                string source = UIPrefabsCustomMenuGenerator.BuildSource(settings);
                string prefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_prefab));

                Assert.That(source, Does.Contain(
                    "[MenuItem(\"GameObject/>>>UI_Prefab/Custom/Reward\", false, 20)]"));
                Assert.That(source, Does.Contain(
                    "[MenuItem(\"GameObject/>>>UI_Prefab/Event/GridReward\", false, 20)]"));
                Assert.That(source, Does.Contain(
                    $"InstantiateRegisteredPrefabByGuid(\"{prefabGuid}\")"));
                Assert.That(source, Does.Contain("CanInstantiateUnderSelection()"));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void InstantiateRegisteredPrefabByGuidCreatesLinkedPrefabUnderSelectedCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var parentObject = new GameObject("Target", typeof(RectTransform));
            parentObject.transform.SetParent(canvasObject.transform, false);
            try
            {
                Selection.activeTransform = parentObject.transform;
                string prefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_prefab));

                NewUIPrefabObject.InstantiateRegisteredPrefabByGuid(prefabGuid);

                GameObject instance = Selection.activeGameObject;
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance.transform.parent, Is.SameAs(parentObject.transform));
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(instance), Is.SameAs(_prefab));

                Undo.PerformUndo();
                Assert.That(instance == null, Is.True);
            }
            finally
            {
                Selection.activeObject = null;
                Object.DestroyImmediate(canvasObject);
            }
        }

        [Test]
        public void GeneratedSourceSkipsDuplicateCustomMenuPaths()
        {
            UIPrefabsSO settings = ScriptableObject.CreateInstance<UIPrefabsSO>();
            try
            {
                settings.Custom = new[]
                {
                    new UIPrefabsSO.Entry { Label = "Reward", Category = "Custom", Prefab = _prefab },
                    new UIPrefabsSO.Entry { Label = "Reward", Category = "Custom", Prefab = _prefab },
                };

                string source = UIPrefabsCustomMenuGenerator.BuildSource(settings);
                const string createMenu =
                    "[MenuItem(\"GameObject/>>>UI_Prefab/Custom/Reward\", false, 20)]";

                Assert.That(CountOccurrences(source, createMenu), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }
    }
}
