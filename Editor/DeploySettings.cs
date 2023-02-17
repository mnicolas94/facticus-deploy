using System.IO;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Deploy.Editor
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        [SerializeField] private string _workflowId;
        [SerializeField] private string _defaultBranch;
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";

        public string WorkflowId
        {
            get => _workflowId;
            set => _workflowId = value;
        }

        public string DefaultBranch => _defaultBranch;

        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;


        public static DeploySettings GetOrCreate()
        {
            if (Instance == null)
            {
                // create directory
                var dir = "Assets/Editor/Deploy";
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();

                // create asset
                var settings = CreateInstance<DeploySettings>();
                var path = Path.Join(dir, "DeploySettings.asset");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }

            return Instance;
        }
    }
}