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

        public IBuildPlatform BuildPlatform => _buildPlatform;

        public IDeployPlatform DeployPlatform => _deployPlatform;

        public override string ToString()
        {
            return $"{_buildPlatform.GameCiName} -> {_deployPlatform.PlatformName}";
        }
    }
}