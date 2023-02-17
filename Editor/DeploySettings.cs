using UnityEngine;
using Utils;

namespace Deploy.Editor
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        [SerializeField] private string _workflowId;
        [SerializeField] private string _defaultBranch;
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";

        public string WorkflowId => _workflowId;

        public string DefaultBranch => _defaultBranch;

        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;
    }
}