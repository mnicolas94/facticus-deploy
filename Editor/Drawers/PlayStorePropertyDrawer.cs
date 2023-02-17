using Deploy.Editor.DeployPlatforms;
using UnityEditor;
using UnityEngine;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PlayStore))]
    public class PlayStorePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var foldoutRect = new Rect(position);
            foldoutRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                label);
            
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            if (property.isExpanded)
            {
                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var iterator = property;
                var parentPath = iterator.propertyPath;
                bool isChild = true;
                bool nextVisible = true;
                while (iterator.NextVisible(nextVisible) && isChild)
                {
                    nextVisible = false;
                    isChild = iterator.propertyPath.StartsWith(parentPath);
                    if (iterator.name == "m_Script" || !isChild)
                    {
                        continue;
                    }

                    var labelString = ObjectNames.NicifyVariableName(iterator.name);
                    var height = EditorGUI.GetPropertyHeight(iterator);
                    var rects = new Rect(position.x, position.y + yOffset, position.width, height);
                    yOffset += height + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.PropertyField(rects, iterator, new GUIContent(labelString), true);
                }
            }
            EditorGUI.indentLevel = indent;
        }
    }
}