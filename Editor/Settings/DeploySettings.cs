﻿using System;
using System.Collections.Generic;
using System.IO;
using Deploy.Editor.BackEnds;
using Deploy.Editor.NotifyPlatforms;
using Deploy.Editor.Versioning;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Deploy.Editor.Settings
{
    public class DeploySettings : ScriptableObjectSingleton<DeploySettings>
    {
        [SerializeField] private string _workflowId;
        [SerializeField, Tooltip("Update the workflow file automatically on package update. It will update the version " +
                                 "of actions used to build and deploy your project. Remember to commit and push the changes" +
                                 " to the workflow file before clicking 'Build and Deploy'")]
        private bool _updateWorkflowAutomatically;
        [SerializeField] private string _gitDirectory;
        [SerializeField] private string _defaultAssetDirectory = "Assets/Editor/Deploy";
        [SerializeReference, SubclassSelector] private ICicdBackend _backend;
        [SerializeReference, SubclassSelector] private INotifyPlatform _notifyPlatform;
        [SerializeField] private VersioningStrategy _versioningStrategy;

        public string WorkflowId
        {
            get => _workflowId;
            set => _workflowId = value;
        }

        public bool UpdateWorkflowAutomatically => _updateWorkflowAutomatically;

        public string GitDirectory => _gitDirectory;

        public string DefaultAssetDirectory => 
            string.IsNullOrEmpty(_defaultAssetDirectory) ? "Assets": _defaultAssetDirectory;

        public ICicdBackend Backend => _backend;

        public INotifyPlatform NotifyPlatform => _notifyPlatform;

        public VersioningStrategy VersioningStrategy => _versioningStrategy;

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
        }
    }
}