﻿using System;
using System.Collections.Generic;
using Deploy.Runtime;
using UnityEngine;

namespace Deploy.Editor.Data
{
    [Serializable]
    public class BuildVariableValue
    {
        [SerializeReference] private ScriptableObject _variable;
        [SerializeReference] private ScriptableObject _value;

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
}