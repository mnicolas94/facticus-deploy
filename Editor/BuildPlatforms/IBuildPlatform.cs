using System;
using UnityEngine;

namespace Deploy.Editor.BuildPlatforms
{
    public interface IBuildPlatform
    {
        string GameCiName => default;
    }
    
    [Serializable]
    public class Windows : IBuildPlatform
    {
        public string GameCiName => "StandaloneWindows64";
    }
    
    [Serializable]
    public class Linux : IBuildPlatform
    {
        public string GameCiName => "StandaloneLinux64";
    }
    
    [Serializable]
    public class Mac : IBuildPlatform
    {
        public string GameCiName => "StandaloneOSX";
    }
    
    [Serializable]
    public class Android : IBuildPlatform
    {
        public string GameCiName => "Android";

        [SerializeField, Tooltip("Set this flag to true to build '.aab' instead of '.apk'")]
        private bool appBundle;
    }
    
    [Serializable]
    public class WebGL : IBuildPlatform
    {
        public string GameCiName => "WebGL";
    }
}