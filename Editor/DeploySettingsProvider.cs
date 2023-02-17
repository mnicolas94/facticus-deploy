using System.IO;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using Utils.Editor.EditorGUIUtils;

namespace Deploy.Editor
{
    public static class DeploySettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider()
        {
            var settings = DeploySettings.GetOrCreate();
            SerializedObject so = new SerializedObject(settings);
            var keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(so);
            
            var provider = new SettingsProvider("Project/Facticus/Deploy", SettingsScope.Project)
            {
                activateHandler = (searchContext, root) =>
                {
                    var inspector = new InspectorElement(settings);
                    root.Add(inspector);
                },
                keywords = keywords
            };
            
            return provider;
        }
    }
}