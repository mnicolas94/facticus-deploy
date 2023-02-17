using UnityEditor;
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
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space(12);
                    
                    if (existsSettings)
                        GUIUtils.DrawSerializedObject(so);
                    else
                    {
                        var r = EditorGUILayout.GetControlRect();
                        if (GUI.Button(r, "Create settings"))
                        {
                            var settings = ScriptableObject.CreateInstance<DeploySettings>();
                            AssetDatabase.CreateAsset(settings, "Assets/DeploySettings.asset");
                        }
                    }
                },
                keywords = keywords
            };
            
            return provider;
        }
    }
}