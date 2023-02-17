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
            bool existsSettings = DeploySettings.Instance != null;
            SerializedObject so = existsSettings ? new SerializedObject(DeploySettings.Instance) : null;
            var keywords = existsSettings ? SettingsProvider.GetSearchKeywordsFromSerializedObject(so) : new string[0];
            var provider = new SettingsProvider("Project/Facticus/Deploy", SettingsScope.Project)
            {
                activateHandler = (searchContext, root) =>
                {
                    var settings = DeploySettings.GetOrCreate();
                    var inspector = new InspectorElement(settings);
                    root.Add(inspector);
                },
                // guiHandler = (searchContext) =>
                // {
                //     EditorGUILayout.Space(12);
                //     
                //     if (existsSettings)
                //         GUIUtils.DrawSerializedObject(so);
                //     else
                //     {
                //         var r = EditorGUILayout.GetControlRect();
                //         if (GUI.Button(r, "Create settings"))
                //         {
                //             var settings = ScriptableObject.CreateInstance<DeploySettings>();
                //             AssetDatabase.CreateAsset(settings, "Assets/DeploySettings.asset");
                //         }
                //     }
                // },
                keywords = keywords
            };
            
            return provider;
        }
    }
}