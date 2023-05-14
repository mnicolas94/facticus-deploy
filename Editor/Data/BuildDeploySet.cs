﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Deploy.Editor.Data
{
    [CreateAssetMenu(fileName = "BuildDeploySet", menuName = "Facticus/Deploy/BuildDeploySet", order = 0)]
    public class BuildDeploySet : ScriptableObject
    {
        [SerializeField] private string _repositoryBranch;
        
        [FormerlySerializedAs("_variables")] [SerializeField]
        private List<BuildVariableValue> _overrideVariables;
        
        [FormerlySerializedAs("_elements")] [SerializeField]
        private List<BuildDeployElement> _platforms;

        public ReadOnlyCollection<BuildVariableValue> OverrideVariables => _overrideVariables.AsReadOnly();

        public ReadOnlyCollection<BuildDeployElement> Platforms => _platforms.AsReadOnly();

        public string RepositoryBranch => _repositoryBranch;
        
        [ContextMenu("Debug build locally")]
        public void BuildLocally()
        {
            BuildDeploy.BuildAndDeploySetLocally(this);
        }

        [ContextMenu("Debug")]
        public void DebugVariables()
        {
            foreach (var variableValue in _overrideVariables)
            {
                Debug.Log($"VariableValue: {JsonUtility.ToJson(variableValue)}");
            }
        }
    }
}