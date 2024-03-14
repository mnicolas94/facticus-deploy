using System.Collections.Generic;
using Deploy.Editor.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deploy.Tests.Editor
{
    public class OverrideVariablesTests
    {
        [Test]
        public void OverrideVariablesTestsSimplePasses()
        {
            // arrange
            var variable = TestScriptableObject.Get(1, 2, "qwe");
            var overrideValue = TestScriptableObject.Get(4, 5, "asd");
            
            // create asset file for it to have guid
            var assetPath = "Assets/Deploy/Tests/Editor/test.asset";
            AssetDatabase.CreateAsset(variable, assetPath);
            AssetDatabase.Refresh();
            
            var tuple = new BuildVariableValue()
            {
                Variable = variable,
                Value = overrideValue,
            };
            var variables = new List<BuildVariableValue>()
            {
                tuple
            };

            // act
            var base64 = variables.OverrideVariablesToBase64();
            OverrideVariablesListExtensions.ApplyOverrideVariablesValues(base64);
            
            // assert
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