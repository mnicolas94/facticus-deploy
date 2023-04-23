using System;
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
}