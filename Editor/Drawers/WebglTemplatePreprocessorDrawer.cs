using Deploy.Editor.BuildPreprocessors.WebGLTemplateSelector;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(WebglTemplatePreprocessor))]
    public class WebglTemplatePreprocessorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);

            var childrenProperties = PropertiesUtils.GetSerializedProperties(property);

            var yOffset = 0f;
            foreach (var childrenProperty in childrenProperties)
            {
                var rect = new Rect(position);
                rect.y += yOffset;
                rect.height = EditorGUI.GetPropertyHeight(childrenProperty);
                EditorGUI.PropertyField(rect, childrenProperty);

                yOffset += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var childrenProperties = PropertiesUtils.GetSerializedProperties(property);

            var height = 0f;
            foreach (var childrenProperty in childrenProperties)
            {
                height += EditorGUI.GetPropertyHeight(childrenProperty);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            
            return height;
        }
    }
}