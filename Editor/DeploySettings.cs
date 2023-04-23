using System.IO;
using Deploy.Editor.NotifyPlatforms;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Deploy.Editor
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        [SerializeField] private string _workflowId;
        [SerializeField] private string _gitDirectory;
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";
        [SerializeReference, SubclassSelector] private INotifyPlatform _notifyPlatform;

        public string WorkflowId
        {
            get => _workflowId;
            set => _workflowId = value;
        }

        public string GitDirectory => _gitDirectory;

        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;

        public INotifyPlatform NotifyPlatform => _notifyPlatform;

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
                var path = Path.Combine(dir, "DeploySettings.asset");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }

            return Instance;
        }
    }
}