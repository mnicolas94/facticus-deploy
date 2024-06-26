﻿using System;
using UnityEngine;

namespace Deploy.Editor.BuildPlatforms
{
    public interface IBuildPlatform
    {
        string GetGameCiName();
    }
    
    [Serializable]
    [AddTypeMenu("Dummy (does not build, only for tests)")]
    public class Dummy : IBuildPlatform
    {
        public string GetGameCiName() => "Dummy";

        [SerializeField] private string dummyDirectory;
    }
    
    [Serializable]
    public class Windows : IBuildPlatform
    {
        public string GetGameCiName() => "StandaloneWindows64";
    }
    
    [Serializable]
    public class Linux : IBuildPlatform
    {
        public string GetGameCiName() => "StandaloneLinux64";
    }
    
    [Serializable]
    public class Mac : IBuildPlatform
    {
        public string GetGameCiName() => "StandaloneOSX";
        
        [SerializeField, Tooltip("Whether to add executable permissions to the app")]
        private bool macAddPermissions = true;
    }
    
    [Serializable]
    public class Android : IBuildPlatform
    {
        public string GetGameCiName() => "Android";

        [SerializeField, Tooltip("Set this flag to true to build '.aab' instead of '.apk'")]
        private bool appBundle;

        public bool AppBundle => appBundle;
    }
    
    [Serializable]
    public class WebGL : IBuildPlatform
    {
        public string GetGameCiName() => "WebGL";
    }
}