using System;
using System.Collections.Generic;
using Deploy.Runtime;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Deploy.Editor.Data
{
    public enum ValueMode
    {
        Preset,
        ScriptableObject_Deprecated,
    }
    
    [Serializable]
    public class BuildVariableValue
    {
        [SerializeField] private ScriptableObject _variable;
        [SerializeField] private ScriptableObject _value;
        [SerializeField] private Preset _preset;
        [SerializeField] private ValueMode _valueMode;

        public ScriptableObject Variable
        {
            get => _variable;
            set => _variable = value;
        }

        public ScriptableObject Value
        {
            get => _value;
            set => _value = value;
        }
    }
    
    [Serializable]
    public class BuildVariableValueJsonSerializable
    {
        [SerializeField] private string _variableGuid;
        [SerializeField] private string _valueJson;

        public string VariableGuid => _variableGuid;

        public string ValueJson => _valueJson;

        public BuildVariableValueJsonSerializable() : this("", "")
        {
        }

        public BuildVariableValueJsonSerializable(string variableGuid, string valueJson)
        {
            _variableGuid = variableGuid;
            _valueJson = valueJson;
        }
    }

    [Serializable]
    public class BuildVariableValueJsonSerializableList
    {
        [SerializeField] private List<BuildVariableValueJsonSerializable> _serializedVariables;

        public List<BuildVariableValueJsonSerializable> SerializedVariables => _serializedVariables;

        public BuildVariableValueJsonSerializableList()
        {
        }

        public BuildVariableValueJsonSerializableList(List<BuildVariableValueJsonSerializable> serializedVariables)
        {
            _serializedVariables = serializedVariables;
        }
    }

    public static class OverrideVariablesListExtensions
    {
        public static void ApplyOverrideVariablesValues(string encodedVariables)
        {
            var variables = OverrideVariablesFromBase64(encodedVariables);
            
            foreach (var serializableVariable in variables)
            {
                var guid = serializableVariable.VariableGuid;
                var valueJson = serializableVariable.ValueJson;

                var variablePath = AssetDatabase.GUIDToAssetPath(guid);
                var variable = AssetDatabase.LoadMainAssetAtPath(variablePath);
                JsonUtility.FromJsonOverwrite(valueJson, variable);
                EditorUtility.SetDirty(variable);
            }
            
            AssetDatabase.SaveAssets();
        }
        
        public static string OverrideVariablesToBase64(this List<BuildVariableValue> variables)
        {
            var serializableVariables = variables.ConvertAll(variableValue =>
            {
                var variable = variableValue.Variable;
                var value = variableValue.Value;
                var path = AssetDatabase.GetAssetPath(variable);
                var guid = AssetDatabase.AssetPathToGUID(path);
                var valueJson = JsonUtility.ToJson(value);
                var serializable = new BuildVariableValueJsonSerializable(guid, valueJson);
                return serializable;
            });
            var serializableList = new BuildVariableValueJsonSerializableList(serializableVariables);
            
            var json = JsonUtility.ToJson(serializableList);
            
            // encode with base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(plainTextBytes);
            
            return base64;
        }
        
        public static List<BuildVariableValueJsonSerializable> OverrideVariablesFromBase64(string base64Encoded)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64Encoded);
            var buildVariables = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            var serializableVariables = JsonUtility.FromJson<BuildVariableValueJsonSerializableList>(buildVariables);

            return serializableVariables.SerializedVariables;
        }
        
        public static string OriginalVariablesToBase64(this List<BuildVariableValue> variables)
        {
            var serializableVariables = variables.ConvertAll(variableValue =>
            {
                var variable = variableValue.Variable;
                var value = variableValue.Variable;
                var path = AssetDatabase.GetAssetPath(variable);
                var guid = AssetDatabase.AssetPathToGUID(path);
                var valueJson = JsonUtility.ToJson(value);
                var serializable = new BuildVariableValueJsonSerializable(guid, valueJson);
                return serializable;
            });
            var serializableList = new BuildVariableValueJsonSerializableList(serializableVariables);
            
            var json = JsonUtility.ToJson(serializableList);
            
            // encode with base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(plainTextBytes);
            
            return base64;
        }
    }
}