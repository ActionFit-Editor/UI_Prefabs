using NUnit.Framework;
using UnityEditor;

namespace ActionFit.UIPrefabs.Editor.Tests
{
    public class UIPrefabsScriptIdentityTests
    {
        [TestCase("026c3fa14e69b4bf38e1418f0ddede6b", "UIPrefabsSO.cs", typeof(UIPrefabsSO))]
        [TestCase("48be464fb55ad4214bd1816b11ea2fee", "NewUIPrefabObject.cs", typeof(NewUIPrefabObject))]
        [TestCase("1dece73f0ea94e2c9e3ba5be1ba6f013", "UIPrefabsSettingsUtility.cs", typeof(UIPrefabsSettingsUtility))]
        public void ScriptPreservesPathTypeAndAssembly(string guid, string fileName, System.Type expectedType)
        {
            string expectedPath = $"Packages/com.actionfit.ui.prefabs/Editor/Scripts/{fileName}";
            Assert.That(AssetDatabase.GUIDToAssetPath(guid), Is.EqualTo(expectedPath));
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(expectedPath);
            Assert.That(script, Is.Not.Null);
            Assert.That(script.GetClass(), Is.EqualTo(expectedType));
            Assert.That(expectedType.Assembly.GetName().Name, Is.EqualTo("com.actionfit.ui.prefabs.Editor"));
        }

        [Test]
        public void MigratedProjectSettingsAssetPreservesGuidWhenPresent()
        {
            const string settingsGuid = "a9aa53af2978e4e7fbbd9722ad0eb3a1";
            string path = AssetDatabase.GUIDToAssetPath(settingsGuid);
            if (string.IsNullOrEmpty(path)) Assert.Ignore("Consuming fixture has no migrated Cat Merge Cafe settings asset.");
            Assert.That(path, Is.EqualTo(UIPrefabsSettingsUtility.DefaultAssetPath));
        }
    }
}
