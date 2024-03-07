using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utils.Attributes;

namespace Deploy.Editor.BuildPreprocessors.WebGLTemplateSelector
{
    public class SerializableWebGLTemplate : ScriptableObject
    {
        [SerializeField, Dropdown(nameof(GetTemplates))] private string _template = "APPLICATION:Default";
        public string Template => _template;

        private DropdownList<string> GetTemplates()
        {
            var templates = new DropdownList<string>();

            templates.Add("Default", "APPLICATION:Default");
            templates.Add("Minimal", "APPLICATION:Minimal");
            templates.Add("PWA", "APPLICATION:PWA");

            var customTemplatesDirectory = "Assets/WebGLTemplates/";
            if (Directory.Exists(customTemplatesDirectory))
            {
                var templatesDirs = Directory.GetDirectories(customTemplatesDirectory);
                foreach (var templateDir in templatesDirs)
                {
                    var fullPath = Path.GetFullPath(templateDir);
                    var splits = fullPath.Split(Path.DirectorySeparatorChar);
                    var templateName = splits[^1];
                    templates.Add(templateName, $"PROJECT:{templateName}");
                }
            }

            return templates;
        }
    }
}