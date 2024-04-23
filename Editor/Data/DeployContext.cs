using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public bool AllDisabled => _platforms.TrueForAll(element => !element.Enabled);
        
#if UNITY_EDITOR
        [ContextMenu(nameof(ChangeToPresetMode))]
        private void ChangeToPresetMode()
        {
            foreach (var variableValue in _overrideVariables)
            {
                variableValue.Editor_ChangeToPresetMode(this);
            }
        }
#endif
    }
}