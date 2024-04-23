using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    public class ScriptableObjectValueElement : VisualElement
    {
        private InspectorElement _valueField;
        private SerializedProperty _property;
        private SerializedProperty _variableProperty;
        private SerializedProperty _valueProperty;

        public ScriptableObjectValueElement(SerializedProperty property, SerializedProperty variableProperty)
        {
            _property = property;
            _variableProperty = variableProperty;
            _valueProperty = property.FindPropertyRelative("_value");

            var valueObject = GetOrCreateValueObject();
            DrawValue(valueObject);
        }

        private void DrawValue(ScriptableObject valueObject)
        {
            if (valueObject != null)
            {
                if (_valueField == null)
                {
                    _valueField = new InspectorElement(valueObject);
                    Add(_valueField);
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
                    if (Contains(_valueField))
                    {
                        Remove(_valueField);
                    }
                    _valueField = null;
                }
            }
        }

        private ScriptableObject GetVariableObject()
        {
            var variableObject = PropertiesUtils.GetTargetObjectOfProperty(_variableProperty) as ScriptableObject;
            return variableObject;
        }

        private ScriptableObject GetValueObject()
        {
            var valueObject = PropertiesUtils.GetTargetObjectOfProperty(_valueProperty) as ScriptableObject;
            return valueObject;
        }
        
        private ScriptableObject GetOrCreateValueObject()
        {
            var valueObject = GetValueObject();

            if (valueObject == null)
            {
                if (!TryCreateNewValue(out valueObject))
                {
                    return null;
                }
            }
            
            return valueObject;
        }

        private bool TryCreateNewValue(out ScriptableObject valueObject)
        {
            var variable = GetVariableObject();
            if (variable == null)
            {
                valueObject = null;
                return false;
            }

            valueObject = Object.Instantiate(variable);
            AddAsSubAsset(_property, valueObject);
            _valueProperty.objectReferenceValue = valueObject;
            _valueProperty.serializedObject.ApplyModifiedProperties();

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

        public void OnVariableChange()
        {
            var variable = GetVariableObject();
            var value = GetValueObject();

            if (variable == null)
            {
                if (value != null)  // remove value because variable is null
                {
                    RemoveFromSubAssets(_property, value);
                    _valueProperty.objectReferenceValue = null;
                    _property.serializedObject.ApplyModifiedProperties();
                    DrawValue(null);
                }
            }
            else
            {
                if (value == null)  // create a value for the variable
                {
                    TryCreateNewValue(out value);
                    DrawValue(value);
                }
                else
                {
                    var variableType = variable.GetType();
                    var valueType = value.GetType();
                    bool sameType = variableType == valueType;
                    if (!sameType)  // draw a new value with the new type
                    {
                        RemoveFromSubAssets(_property, value);
                        TryCreateNewValue(out value);
                        DrawValue(value);
                    }
                }
            }
        }
    }
}