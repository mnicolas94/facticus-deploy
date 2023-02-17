using System;
using UnityEngine;

namespace Deploy.Editor
{
    [Serializable]
    internal class DeployPersistentSettings
    {
        [SerializeField] private string _githubAuthToken;

        public string GithubAuthToken
        {
            get => _githubAuthToken;
            set => _githubAuthToken = value;
        }
    }
}