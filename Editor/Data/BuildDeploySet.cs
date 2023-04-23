using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace Deploy.Editor.Data
{
    [CreateAssetMenu(fileName = "BuildDeploySet", menuName = "Facticus/Deploy/BuildDeploySet", order = 0)]
    public class BuildDeploySet : ScriptableObject
    {
        [SerializeField] private string _repositoryBranch;
        [SerializeField] private List<BuildVariableValue> _variables;
        [SerializeField] private List<BuildDeployElement> _elements;

        public ReadOnlyCollection<BuildVariableValue> Variables => _variables.AsReadOnly();

        public ReadOnlyCollection<BuildDeployElement> Elements => _elements.AsReadOnly();

        public string RepositoryBranch => _repositoryBranch;
        
        [ContextMenu("Debug build locally")]
        public void BuildLocally()
        {
            BuildDeploy.BuildAndDeploySetLocally(this);
        }

        [ContextMenu("Debug")]
        public void DebugVariables()
        {
            foreach (var variableValue in _variables)
            {
                Debug.Log($"VariableValue: {JsonUtility.ToJson(variableValue)}");
            }
        }
    }
}