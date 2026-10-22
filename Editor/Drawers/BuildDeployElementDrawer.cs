using System;
using System.Collections.Generic;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utils.Editor;
using Utils.Editor.EditorGUIUtils;

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

        private IntegerField _fileSizeField;
        
        private readonly string[] _propertiesToUpdateLabel = new[]
        {
            "_buildPlatform",
            "_deployPlatform"
        };

        public BuildDeployElementField(SerializedProperty property)
        {
            _property = property;
            
            var propertyToVisualElementMap = new Dictionary<string, Func<SerializedProperty, VisualElement>>
            {
                {"_buildPlatform", GetPlatformsVisualElements},
                {"_deployPlatform", GetPlatformsVisualElements},
                {"_createSwapFile", GetSwapFileVisualElement},
                {"_swapFileSizeGb", GetSwapFileSizeVisualElement},
            };
            
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
            var serializedProperties = PropertiesUtils.GetSerializedProperties(property);
            GUIUtils.DrawSerializedProperties(_propertiesContainer, serializedProperties, propertyToVisualElementMap);
            
            Add(labelContainer);
            Add(_propertiesContainer);
        }

        private VisualElement GetPlatformsVisualElements(SerializedProperty sp)
        {
            var propertyField = new PropertyField(sp);

            // update label when platform values change
            propertyField.RegisterValueChangeCallback(OnAnyPlatformChanged);

            return propertyField;
        }

        private VisualElement GetSwapFileSizeVisualElement(SerializedProperty property)
        {
            _fileSizeField = new IntegerField();
            _fileSizeField.label = property.displayName;
            _fileSizeField.BindProperty(property);
            return _fileSizeField;
        }

        private VisualElement GetSwapFileVisualElement(SerializedProperty property)
        {
            var propertyField = new Toggle();
            propertyField.label = property.displayName;
            propertyField.BindProperty(property);
            propertyField.RegisterValueChangedCallback(OnSwapFileToggleChanged);
            return propertyField;
        }

        private void OnSwapFileToggleChanged(ChangeEvent<bool> changeEvent)
        {
            if (_fileSizeField == null) return;
            _fileSizeField.style.display = changeEvent.newValue ? DisplayStyle.Flex : DisplayStyle.None;
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