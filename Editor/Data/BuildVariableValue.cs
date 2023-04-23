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

        public ScriptableObject Variable => _variable;

        public ScriptableObject Value => _value;
    }
}