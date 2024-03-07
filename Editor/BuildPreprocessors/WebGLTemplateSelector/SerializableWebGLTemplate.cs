using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils.Attributes;

namespace Deploy.Editor.BuildPreprocessors.WebGLTemplateSelector
{
    public class SerializableWebGLTemplate : ScriptableObject
    {
        [SerializeField, Dropdown(nameof(GetTemplates))] private string _template = "APPLICATION:Default";
        public string Template => _template;

        [SerializeField, HideInInspector] private List<(string, string)> _templates = new();
        
        private void OnValidate()
        {
            if (_templates == null || _templates.Count == 0)
            {
                ResetToDefault();
            }
        }

        private DropdownList<string> GetTemplates()
        {
            var templates = new DropdownList<string>();

            foreach (var (displayName, template) in _templates)
            {
                templates.Add(displayName, template);
            }

            return templates;
        }

        [ContextMenu("Reset to default templates")]
        private void ResetToDefault()
        {
            _templates = new()
            {
                ("Default", "APPLICATION:Default"),
                ("Minimal", "APPLICATION:Minimal"),
                ("PWA", "APPLICATION:PWA"),
            };
        }
        
        [ContextMenu("Add current template to list")]
        private void AddCurrentTemplate()
        {
            var template = PlayerSettings.WebGL.template;
            var elements = template.Split(':');
            var displayName = elements[^1];
            _templates.Add((displayName, template));
            EditorUtility.SetDirty(this);
        }
    }
}