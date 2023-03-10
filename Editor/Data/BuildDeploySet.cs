using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace Deploy.Editor.Data
{
    [CreateAssetMenu(fileName = "BuildDeploySet", menuName = "Facticus/Deploy/BuildDeploySet", order = 0)]
    public class BuildDeploySet : ScriptableObject
    {
        [SerializeField] private string _repositoryBranch;
        [SerializeField] private List<BuildDeployElement> _elements;

        public ReadOnlyCollection<BuildDeployElement> Elements => _elements.AsReadOnly();

        public string RepositoryBranch => _repositoryBranch;
        
        [ContextMenu("Debug build locally")]
        public void BuildLocally()
        {
            BuildDeploy.BuildAndDeploySetLocally(this);
        }
    }
}