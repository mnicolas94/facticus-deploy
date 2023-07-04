using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deploy.Editor.Settings
{
    [Serializable]
    [MovedFrom("Deploy.Editor")]
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