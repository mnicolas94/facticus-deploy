using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BuildVariableValue))]
    public class BuildVariableValueDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new BuildVariableValueElement(property);
        }
    }
    
    public class BuildVariableValueElement : VisualElement
    {
        private SerializedProperty _property;
        private SerializedProperty _variableProperty;
        private SerializedProperty _modeProperty;

        private ScriptableObjectValueElement _soDrawer;
        private PresetValueElement _presetDrawer;

        private bool IsPreset => GetMode() == ValueMode.Preset;

        public BuildVariableValueElement(SerializedProperty property)
        {
            _property = property;
            _variableProperty = property.FindPropertyRelative("_variable");
            _modeProperty = property.FindPropertyRelative("_valueMode");
            
            // variable
            var variableField = new PropertyField(_variableProperty);
            variableField.RegisterValueChangeCallback(OnVariableChange);
            Add(variableField);
            
            // mode
            var modeField = new PropertyField(_modeProperty);
            modeField.RegisterValueChangeCallback(OnModeChange);
            Add(modeField);

            // value
            _soDrawer = new ScriptableObjectValueElement(_property, _variableProperty);
            _presetDrawer = new PresetValueElement(_property, _variableProperty);
            Add(_soDrawer);
            Add(_presetDrawer);
            SetDrawerVisibility();
        }

        private ValueMode GetMode()
        {
            return (ValueMode)_modeProperty.enumValueIndex;
        }

        private void SetDrawerVisibility()
        {
            _soDrawer.style.display = IsPreset ? DisplayStyle.None : DisplayStyle.Flex;
            _presetDrawer.style.display = IsPreset ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnVariableChange(SerializedPropertyChangeEvent evt)
        {
            if (IsPreset)
            {
                _presetDrawer.OnVariableChange();
            }
            else
            {
                _soDrawer.OnVariableChange();
            }
        }
        
        private void OnModeChange(SerializedPropertyChangeEvent evt)
        {
            SetDrawerVisibility();
            if (IsPreset)
            {
                _presetDrawer.OnVariableChange();
            }
            else
            {
                _soDrawer.OnVariableChange();
            }
        }
    }
}