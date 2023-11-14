using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Deploy.Editor.BackEnds;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace Deploy.Editor.Data
{
    [CreateAssetMenu(fileName = "DeployContext", menuName = "Facticus/Deploy/DeployContext", order = 0)]
    [MovedFrom(false, null, null, sourceClassName: "BuildDeploySet")]
    public class DeployContext : ScriptableObject
    {
        [FormerlySerializedAs("_repositoryBranch")] [SerializeField]
        private string _repositoryBranchOrTag;
        
        [FormerlySerializedAs("_variables")] [SerializeField]
        private List<BuildVariableValue> _overrideVariables;
        
        [FormerlySerializedAs("_elements")] [SerializeField]
        private List<BuildDeployElement> _platforms;

        public ReadOnlyCollection<BuildVariableValue> OverrideVariables => _overrideVariables.AsReadOnly();

        public ReadOnlyCollection<BuildDeployElement> Platforms => _platforms.AsReadOnly();

        public string RepositoryBranchOrTag => _repositoryBranchOrTag;

        [ContextMenu("Debug")]
        public void DebugVariables()
        {
            var inputs = GithubActionsBackend.GetListBuildSetInput(_platforms.AsReadOnly(), _overrideVariables);
            Debug.Log($"{string.Join("\n", inputs)}");
        }
        
        [ContextMenu("Debug call")]
        public async void DebugWorkflowCall()
        {
            var ci = new GithubActionsBackend();
            var success = ci.BuildAndDeploy(this);
            Debug.Log($"{success}");
        }

        public bool AllDisabled => _platforms.TrueForAll(element => !element.Enabled);
    }
}