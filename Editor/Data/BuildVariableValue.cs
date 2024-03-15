using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public ScriptableObject Variable => _variable;

        [SerializeField] private ScriptableObject _value;
        public ScriptableObject Value => _value;

        [SerializeField] private Preset _preset;

        [SerializeField] private ValueMode _valueMode;

        public ScriptableObject OverrideVariable
        {
            get
            {
                ScriptableObject overrideValue = _value;
                if (_valueMode == ValueMode.Preset)
                {
                    overrideValue = Object.Instantiate(_variable);
                    _preset.ApplyTo(overrideValue);
                }

                return overrideValue;
            }
        }

        public BuildVariableValue(ScriptableObject variable, ScriptableObject value)
        {
            _variable = variable;
            _value = value;
            _valueMode = ValueMode.ScriptableObject_Deprecated;
        }

        public BuildVariableValue(ScriptableObject variable, Preset preset)
        {
            _variable = variable;
            _preset = preset;
            _valueMode = ValueMode.Preset;
        }

        public BuildVariableValueJsonSerializable ToJsonSerializable()
        {
            var overrideValue = OverrideVariable;

            return ToJsonSerializable(overrideValue);
        }

        public BuildVariableValueJsonSerializable ToJsonSerializableBackup()
        {
            return ToJsonSerializable(overrideValue: _variable);
        }

        private BuildVariableValueJsonSerializable ToJsonSerializable(ScriptableObject overrideValue)
        {
            var path = AssetDatabase.GetAssetPath(_variable);
            var guid = AssetDatabase.AssetPathToGUID(path);
            var valueJson = JsonUtility.ToJson(overrideValue);
            var serializable = new BuildVariableValueJsonSerializable(guid, valueJson);
            return serializable;
        }

        public void Clear()
        {
            _variable = null;
            _value = null;
            _preset = null;
            _valueMode = ValueMode.Preset;
        }

#if UNITY_EDITOR
        public void Editor_ChangeToPresetMode(DeployContext parentDeployContext)
        {
            if (_valueMode == ValueMode.Preset)
                return;
            
            var path = AssetDatabase.GetAssetPath(_variable);
            var directory = Path.GetDirectoryName(path);
            var newPresetPath = Path.Combine(directory, $"{_variable.name}.{parentDeployContext.name}.asset");
            var preset = new Preset(_variable);
            AssetDatabase.CreateAsset(preset, newPresetPath);

            _preset = preset;
            _valueMode = ValueMode.Preset;
            
            EditorUtility.SetDirty(parentDeployContext);
        }
#endif
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
            var serializableVariables = variables.ConvertAll(
                variableValue => variableValue.ToJsonSerializable());   
            var serializableList = new BuildVariableValueJsonSerializableList(serializableVariables);
            
            var json = JsonUtility.ToJson(serializableList);
            
            // encode with base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(plainTextBytes);
            
            return base64;
        }
        
        public static string OriginalVariablesToBase64(this List<BuildVariableValue> variables)
        {
            var serializableVariables = variables.ConvertAll(
                variableValue => variableValue.ToJsonSerializableBackup());
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
    }
}