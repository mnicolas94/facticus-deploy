using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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
                    var scroll = new ScrollView(ScrollViewMode.Vertical);
                    scroll.Add(inspector);
                    root.Add(scroll);
                },
                keywords = keywords
            };
            
            return provider;
        }
    }
}