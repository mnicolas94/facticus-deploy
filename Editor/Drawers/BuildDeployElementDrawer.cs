using System.Collections.Generic;
using System.Linq;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BuildDeployElement))]
    public class BuildDeployElementDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new BuildDeployElementField(property);
        }
    }

    public class BuildDeployElementField : VisualElement
    {
        private SerializedProperty _property;
        private Toggle _toggle;
        private Foldout _foldout;
        private Label _foldoutLabel;
        private VisualElement _propertiesContainer;

        private readonly string[] _propertiesToUpdateLabel = new[]
        {
            "_buildPlatform",
            "_deployPlatform"
        };

        public BuildDeployElementField(SerializedProperty property)
        {
            _property = property;
            
            var labelString = GetLabel(property);

            // Enabled toggle
            var enabledProperty = property.FindPropertyRelative("_enabled");
            _toggle = new Toggle();
            _toggle.BindProperty(enabledProperty);
            _toggle.tooltip = enabledProperty.tooltip;
            _toggle.RegisterValueChangedCallback(OnEnabledChanged);
            _toggle.style.marginRight = 3;
            
            // Foldout
            _foldout = new Foldout();
            _foldout.text = labelString;
            _foldout.style.marginLeft = 20;
            _foldout.RegisterValueChangedCallback(OnFoldoutChange);
            _foldoutLabel = _foldout.Q<Label>(className: "unity-foldout__text");
            
            var labelContainer = new VisualElement();
            labelContainer.style.flexDirection = FlexDirection.Row;
            labelContainer.Add(_toggle);
            labelContainer.Add(_foldout);

            _propertiesContainer = new VisualElement();
            _propertiesContainer.style.marginLeft = 30;
            foreach (var sp in PropertiesUtils.GetSerializedProperties(property))
            {
                var propertyField = new PropertyField(sp);
                _propertiesContainer.Add(propertyField);

                // update label when platform values change
                if (_propertiesToUpdateLabel.Contains(sp.name))
                {
                    propertyField.RegisterValueChangeCallback(OnAnyPlatformChanged);
                }
            }
            
            Add(labelContainer);
            Add(_propertiesContainer);
        }

        private void OnAnyPlatformChanged(SerializedPropertyChangeEvent evt)
        {
            UpdateLabel();
        }

        private void OnEnabledChanged(ChangeEvent<bool> evt)
        {
            var enabled = evt.newValue;
            _propertiesContainer.SetEnabled(enabled);
            _foldoutLabel.SetEnabled(enabled);
        }
        
        private void OnFoldoutChange(ChangeEvent<bool> evt)
        {
            var displayStyle = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            _propertiesContainer.style.display = displayStyle;
        }

        private void UpdateLabel()
        {
            var newLabel = GetLabel(_property);
            _foldout.text = newLabel;
        }

        private static string GetLabel(SerializedProperty property)
        {
            string label = property.displayName;
            try
            {
                var target = PropertiesUtils.GetTargetObjectOfProperty(property);
                label = target.ToString();
            }
            catch
            {
                // ignored
            }

            return label;
        }
    }
}