using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BuildDeployElement))]
    public class BuildDeployElementDrawer : PropertyDrawer
    {
        private VisualElement _root;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            string label = null;
            try
            {
                var target = PropertiesUtils.GetTargetObjectOfProperty(property);
                label = target.ToString();
            }
            catch
            {
                // ignored
            }

            _root = new PropertyField(property, label);
            return _root;
        }
    }
}