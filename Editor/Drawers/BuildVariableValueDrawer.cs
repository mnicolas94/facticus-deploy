using System;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<ValueMode, IBuildVariableDrawer> _modeDrawers;
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
            _modeDrawers = new Dictionary<ValueMode, IBuildVariableDrawer>()
            {
                { ValueMode.Preset, new PresetValueElement(_property, _variableProperty) },
                // { ValueMode.ScriptableObject_Deprecated, new ScriptableObjectValueElement(_property, _variableProperty) },
            };

            foreach (var drawer in _modeDrawers.Values)
            {
                Add(drawer as VisualElement);
            }
            
            SetDrawerVisibility();
        }

        private void ConstructModesMenu()
        {
            var modes = Enum.GetValues(typeof(ValueMode)).Cast<ValueMode>().ToList();
            // remove deprecated mode
            modes.Remove(ValueMode.ScriptableObject_Deprecated);
            
            foreach (var mode in modes)
            {
                _modeMenu.menu.AppendAction(
                    mode.ToString(),
                    menuItem =>
                    {
                        var newMode = (ValueMode) menuItem.userData;
                        _modeProperty.enumValueIndex = (int) newMode;
                        _modeProperty.serializedObject.ApplyModifiedProperties();
                        OnModeChange();
                    },
                    menuItem =>
                    {
                        var currentMode = GetMode();
                        var mode = (ValueMode) menuItem.userData;
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
            var currentMode = GetMode();
            foreach (var (mode, drawer) in _modeDrawers)
            {
                var visualElement = drawer as VisualElement;
                visualElement.style.display = mode == currentMode ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnVariableChange(SerializedPropertyChangeEvent evt)
        {
            var mode = GetMode();
            if (_modeDrawers.TryGetValue(mode, out var drawer))
            {
                drawer.OnVariableChange();
            }
        }
        
        private void OnModeChange()
        {
            SetDrawerVisibility();
            var mode = GetMode();
            if (_modeDrawers.TryGetValue(mode, out var drawer))
            {
                drawer.OnVariableChange();
            }
        }
    }

    public interface IBuildVariableDrawer
    {
        void OnVariableChange();
    }
}