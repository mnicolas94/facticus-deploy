using System;
using Deploy.Editor.BuildPlatforms;
using Deploy.Editor.DeployPlatforms;
using UnityEngine;

namespace Deploy.Editor.Data
{
    [Serializable]
    public class BuildDeployElement
    {
        [SerializeField] private bool _developmentBuild;
        [SerializeReference, SubclassSelector] private IBuildPlatform _buildPlatform;
        [SerializeReference, SubclassSelector] private IDeployPlatform _deployPlatform;
        // [SerializeField] private SubclassSelector<IDeployPlatform> _deployPlatformCustom;

        public bool DevelopmentBuild => _developmentBuild;

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