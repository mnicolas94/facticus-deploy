using System.Collections.Generic;
using Deploy.Editor.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Deploy.Tests.Editor
{
    public class OverrideVariablesTests
    {
        private static List<BuildVariableValue> _overrides = new ()
        {
            new BuildVariableValue(
                TestScriptableObject.Get(1, 2, "qwe"),
                TestScriptableObject.Get(4, 5, "asd")),
            new BuildVariableValue(
                TestScriptableObject.Get(1, 2, "qwe"),
                new Preset(TestScriptableObject.Get(4, 5, "asd"))),
        };

        [TestCaseSource(nameof(_overrides))]
        public void When_ApplyOverrideWithBuildVariableValue_OverridesAreAppliedCorrectly(BuildVariableValue overrideVariable)
        {
            // arrange
            // create asset file for it to have guid
            var assetPath = "Assets/Deploy/Tests/Editor/test.asset";
            AssetDatabase.CreateAsset(overrideVariable.Variable, assetPath);
            AssetDatabase.Refresh();

            var variables = new List<BuildVariableValue>()
            {
                overrideVariable
            };

            // false-positive assert
            var variable = overrideVariable.Variable as TestScriptableObject;
            var overrideValue = overrideVariable.OverrideVariable as TestScriptableObject;
            Assert.IsFalse(variable.IsEqualTo(overrideValue));
            
            // act
            var base64 = variables.OverrideVariablesToBase64();
            OverrideVariablesListExtensions.ApplyOverrideVariablesValues(base64);
            
            // assert
            variable = overrideVariable.Variable as TestScriptableObject;
            overrideValue = overrideVariable.OverrideVariable as TestScriptableObject;
            Assert.IsTrue(variable.IsEqualTo(overrideValue));
            
            // teardown
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    public class TestScriptableObject : ScriptableObject
    {
        public int Int;
        public float Float;
        public string String;

        public static TestScriptableObject Get(int i = 0, float f = 0f, string s = "")
        {
            var tso = CreateInstance<TestScriptableObject>();
            tso.Int = i;
            tso.Float = f;
            tso.String = s;
            return tso;
        }

        public bool IsEqualTo(TestScriptableObject other)
        {
            if (Int != other.Int) return false;
            if (!Mathf.Approximately(Float, other.Float)) return false;
            if (String != other.String) return false;

            return true;
        }
    }
}