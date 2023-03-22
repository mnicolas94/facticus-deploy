using UnityEngine;

namespace Deploy.Editor.Utility
{
    internal class TokenDialogInput : ScriptableObject
    {
        [SerializeField] private string _authToken;

        public string AuthToken => _authToken;
    }
}