using System;
using Deploy.Editor.BuildPlatforms;
using Deploy.Editor.DeployPlatforms;
using UnityEngine;

namespace Deploy.Editor.Data
{
    [Serializable]
    public class BuildDeployElement
    {
        [SerializeField, HideInInspector] private bool _enabled = true;
        [SerializeField] private bool _developmentBuild;
        [SerializeField]
        [Tooltip("Whether to free disk space in the runner that builds your project before the build starts. " +
                 "Not applicable to local builds with Act backend. Only set it to true if you are getting a " +
                 "\"...no space left on device.\" error.")]
        private bool _freeDiskSpaceBeforeBuild;
        [SerializeReference, SubclassSelector] private IBuildPlatform _buildPlatform;
        [SerializeReference, SubclassSelector] private IDeployPlatform _deployPlatform;
        // [SerializeField] private SubclassSelector<IDeployPlatform> _deployPlatformCustom;

        public bool Enabled => _enabled;

        public bool DevelopmentBuild => _developmentBuild;

        public bool FreeDiskSpaceBeforeBuild => _freeDiskSpaceBeforeBuild;

        public IBuildPlatform BuildPlatform
        {
            get => _buildPlatform;
            set => _buildPlatform = value;
        }

        public IDeployPlatform DeployPlatform
        {
            get => _deployPlatform;
            set => _deployPlatform = value;
        }

        public override string ToString()
        {
            return $"{_buildPlatform.GetGameCiName()} -> {_deployPlatform.GetPlatformName()}";
        }
    }
}