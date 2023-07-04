using UnityEditor;
using UnityEditor.UIElements;

namespace Deploy.Editor.Settings
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