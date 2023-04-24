using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deploy.Editor.Data
{
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
}