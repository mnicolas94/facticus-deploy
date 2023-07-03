using System.Collections.Generic;
using System.IO;
using Deploy.Editor.BackEnds;
using Deploy.Editor.NotifyPlatforms;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Attributes;

namespace Deploy.Editor
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        [SerializeField] private string _workflowId;
        [SerializeField] private string _gitDirectory;
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";
        [SerializeReference, SubclassSelector] private ICicdBackend _backend;
        [SerializeReference, SubclassSelector] private INotifyPlatform _notifyPlatform;
        [SerializeField, Dropdown(nameof(GetVersioningStrategies))]
        private string _versioningStrategy;

        public string WorkflowId
        {
            get => _workflowId;
            set => _workflowId = value;
        }

        public string GitDirectory => _gitDirectory;

        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;

        public ICicdBackend Backend => _backend;

        public INotifyPlatform NotifyPlatform => _notifyPlatform;

        public string VersioningStrategy => _versioningStrategy;

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
        
        private List<string> GetVersioningStrategies()
        {
            return new List<string>
            {
                "Semantic",
                "Tag",
                "None",
            };
        }
    }
}