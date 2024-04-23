using System;
using System.Collections.Generic;
using System.Linq;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
        private static List<ValueMode> Choices = Enum.GetNames(typeof(ValueMode))
            .Select(enumString => Enum.Parse<ValueMode>(enumString)).ToList();
        
        private SerializedProperty _property;
        private SerializedProperty _variableProperty;
        private SerializedProperty _modeProperty;

        
        private ScriptableObjectValueElement _soDrawer;
        private PresetValueElement _presetDrawer;
        private ToolbarMenu _modeMenu;

        private bool IsPreset => GetMode() == ValueMode.Preset;

        public BuildVariableValueElement(SerializedProperty property)
        {
            _property = property;
            _variableProperty = property.FindPropertyRelative("_variable");
            _modeProperty = property.FindPropertyRelative("_valueMode");

            var variableContainer = new VisualElement();
            variableContainer.style.flexDirection = FlexDirection.Row;
            Add(variableContainer);
            
            // variable
            var variableField = new PropertyField(_variableProperty);
            variableField.style.flexGrow = 1;
            variableField.RegisterValueChangeCallback(OnVariableChange);
            variableContainer.Add(variableField);
            
            // mode
            _modeMenu = new ToolbarMenu();
            ConstructModesMenu();
            variableContainer.Add(_modeMenu);

            // value
            _soDrawer = new ScriptableObjectValueElement(_property, _variableProperty);
            _presetDrawer = new PresetValueElement(_property, _variableProperty);
            Add(_soDrawer);
            Add(_presetDrawer);
            SetDrawerVisibility();
        }

        private void ConstructModesMenu()
        {
            var modes = Enum.GetValues(typeof(ValueMode)).Cast<ValueMode>();
            foreach (var mode in modes)
            {
                _modeMenu.menu.AppendAction(
                    mode.ToString(),
                    menuItem =>
                    {
                        var newMode = ((ValueMode)menuItem.userData);
                        _modeProperty.enumValueIndex = (int)newMode;
                        _modeProperty.serializedObject.ApplyModifiedProperties();
                        OnModeChange();
                    },
                    menuItem =>
                    {
                        var currentMode = GetMode();
                        var mode = ((ValueMode)menuItem.userData);
                        return mode == currentMode ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                    },
                    mode
                );
            }
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
        
        private void OnModeChange()
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