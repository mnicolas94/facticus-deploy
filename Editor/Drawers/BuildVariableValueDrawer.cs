using System.IO;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BuildVariableValue))]
    public class BuildVariableValueDrawer : PropertyDrawer
    {
        private VisualElement _root;
        private InspectorElement _valueField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();
            
            // variable
            var variableField = new PropertyField(property.FindPropertyRelative("_variable"));
            variableField.RegisterValueChangeCallback(OnVariableChange);
            _root.Add(variableField);

            // value
            var valueObject = GetOrCreateValueObject(property);
            DrawValue(valueObject);

            return _root;
        }

        private void DrawValue(ScriptableObject valueObject)
        {
            if (valueObject != null)
            {
                if (_valueField == null)
                {
                    _valueField = new InspectorElement(valueObject);
                    _root.Add(_valueField);
                }
                else
                {
                    _valueField.Bind(new SerializedObject(valueObject));
                }
            }
            else
            {
                if (_valueField != null)
                {
                    if (_root.Contains(_valueField))
                    {
                        _root.Remove(_valueField);
                    }
                    _valueField = null;
                }
            }
        }

        private ScriptableObject GetVariableObject(SerializedProperty property)
        {
            var variableProperty = property.FindPropertyRelative("_variable");
            var variableObject = PropertiesUtils.GetTargetObjectOfProperty(variableProperty) as ScriptableObject;
            return variableObject;
        }

        private ScriptableObject GetValueObject(SerializedProperty property)
        {
            var valueProperty = property.FindPropertyRelative("_value");
            var valueObject = PropertiesUtils.GetTargetObjectOfProperty(valueProperty) as ScriptableObject;
            return valueObject;
        }
        
        private ScriptableObject GetOrCreateValueObject(SerializedProperty property)
        {
            var valueObject = GetValueObject(property);

            if (valueObject == null)
            {
                if (!TryCreateNewValue(property, out valueObject))
                {
                    return null;
                }
            }
            
            return valueObject;
        }

        private bool TryCreateNewValue(SerializedProperty property, out ScriptableObject valueObject)
        {
            var variable = GetVariableObject(property);
            if (variable == null)
            {
                valueObject = null;
                return false;
            }

            valueObject = ScriptableObject.Instantiate(variable);
            var valueProperty = property.FindPropertyRelative("_value");
            AddAsSubAsset(property, valueObject);
            valueProperty.objectReferenceValue = valueObject;
            valueProperty.serializedObject.ApplyModifiedProperties();

            return true;
        }

        private static void AddAsSubAsset(SerializedProperty property, ScriptableObject valueObject)
        {
            var parentAsset = property.serializedObject.targetObject;
            AssetDatabase.AddObjectToAsset(valueObject, parentAsset);
            EditorUtility.SetDirty(parentAsset);
            EditorUtility.SetDirty(valueObject);
            AssetDatabase.SaveAssets();
        }
        
        private static void RemoveFromSubAssets(SerializedProperty property, ScriptableObject valueObject)
        {
            var parentAsset = property.serializedObject.targetObject;
            AssetDatabase.RemoveObjectFromAsset(valueObject);
            EditorUtility.SetDirty(parentAsset);
            AssetDatabase.SaveAssets();
        }

        private void OnVariableChange(SerializedPropertyChangeEvent evt)
        {
            Debug.Log("OnVariableChange");
            var parentProperty = evt.changedProperty.FindParentProperty();
            var variable = GetVariableObject(parentProperty);
            var value = GetValueObject(parentProperty);

            if (variable == null)
            {
                if (value != null)  // remove value because variable is null
                {
                    RemoveFromSubAssets(parentProperty, value);
                    parentProperty.FindPropertyRelative("_value").objectReferenceValue = null;
                    parentProperty.serializedObject.ApplyModifiedProperties();
                    DrawValue(null);
                }
            }
            else
            {
                if (value == null)  // create a value for the variable
                {
                    TryCreateNewValue(parentProperty, out value);
                    DrawValue(value);
                }
                else
                {
                    var variableType = variable.GetType();
                    var valueType = value.GetType();
                    bool sameType = variableType == valueType;
                    if (!sameType)  // draw a new value with the new type
                    {
                        RemoveFromSubAssets(parentProperty, value);
                        TryCreateNewValue(parentProperty, out value);
                        DrawValue(value);
                    }
                }
            }
        }
    }
}