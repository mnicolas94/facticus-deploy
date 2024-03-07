using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Deploy.Editor.BackEnds;
using Deploy.Editor.BuildPreprocessors;
using Deploy.Editor.NotifyPlatforms;
using Deploy.Editor.Versioning;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Deploy.Editor.Settings
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        public static string PropertyNameWorkflowId => nameof(_workflowId);
        public static string PropertyNameOnDemandBuildPreprocessors => nameof(_onDemandBuildPreprocessors);
        
        [SerializeField] private string _workflowId;
        public string WorkflowId
        {
            get => _workflowId;
            set => _workflowId = value;
        }
        
        [SerializeField, Tooltip("Update the workflow file automatically on package update. It will update the version " +
                                 "of actions used to build and deploy your project. Remember to commit and push the changes" +
                                 " to the workflow file before clicking 'Build and Deploy'")]
        private bool _updateWorkflowAutomatically;
        public bool UpdateWorkflowAutomatically => _updateWorkflowAutomatically;
        
        [SerializeField] private string _gitDirectory;
        public string GitDirectory => _gitDirectory;
        
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";
        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;
        
        [SerializeReference, SubclassSelector] private ICicdBackend _backend;
        public ICicdBackend Backend => _backend;
        
        [SerializeReference, SubclassSelector] private INotifyPlatform _notifyPlatform;
        public INotifyPlatform NotifyPlatform => _notifyPlatform;
        
        [SerializeField] private VersioningStrategy _versioningStrategy;
        public VersioningStrategy VersioningStrategy => _versioningStrategy;

        [SerializeReference, SubclassSelector]
        [Tooltip("This is a list of build preprocessors that will only trigger if they are added to this list")]
        private List<IOnDemandBuildPreprocessorWithReport> _onDemandBuildPreprocessors = new();
        public ReadOnlyCollection<IOnDemandBuildPreprocessorWithReport> OnDemandBuildPreprocessors => _onDemandBuildPreprocessors.AsReadOnly();
        
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

        private void OnValidate()
        {
            if (_backend == null)
            {
                _backend = new GithubActionsBackend();
            }

            foreach (var processor in _onDemandBuildPreprocessors)
            {
                processor?.OnValidate();
            }
        }
    }
}