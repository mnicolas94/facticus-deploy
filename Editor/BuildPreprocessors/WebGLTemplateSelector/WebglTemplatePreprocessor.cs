using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Deploy.Editor.BuildPreprocessors.WebGLTemplateSelector
{
    [Serializable]
    public class WebglTemplatePreprocessor : OnDemandBuildPreprocessorWithReport
    {
        private const string TemplateResourcePath = "WebGLTemplateSelector";
        
        [SerializeField] private SerializableWebGLTemplate _templateSelector;

        protected override void OnPreprocessBuildInternal(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                var templateSelector = Resources.Load<SerializableWebGLTemplate>(TemplateResourcePath);
                PlayerSettings.WebGL.template = templateSelector.Template;
            }
        }

        public override void OnValidate()
        {
            EnsureDataExists();
        }

        private void EnsureDataExists()
        {
            if (_templateSelector == null)
            {
                _templateSelector = Resources.Load<SerializableWebGLTemplate>(TemplateResourcePath);
            }
            
            if (_templateSelector == null)
            {
                _templateSelector = ScriptableObject.CreateInstance<SerializableWebGLTemplate>();
                var assetPath = $"Assets/Editor/Resources/{TemplateResourcePath}.asset";
                var assetDir = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(assetDir))
                {
                    Directory.CreateDirectory(assetDir);
                }

                AssetDatabase.CreateAsset(_templateSelector, assetPath);
            }
        }
    }
}