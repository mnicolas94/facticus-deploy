using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deploy.Editor.EditorWindows;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;
using Utils.Editor.GenericSearchWindow;
using Object = UnityEngine.Object;

namespace Deploy.Editor.Drawers
{
    public class PresetValueElement : VisualElement
    {
        private CustomPresetField _presetField;
        private InspectorElement _valueField;
        private Foldout _editFoldout;
        private SerializedProperty _property;
        private SerializedProperty _variableProperty;
        private SerializedProperty _presetProperty;
        private VisualElement _presetContainer;
        private Button _createPresetButton;
        private VisualElement _foldoutHeader;

        public PresetValueElement(SerializedProperty property, SerializedProperty variableProperty)
        {
            _property = property;
            _variableProperty = variableProperty;
            _presetProperty = property.FindPropertyRelative("_preset");

            SetupUI();
            UpdateUI(GetPreset());
        }

        private void SetupUI()
        {
            var variable = GetVariableObject();
            var preset = GetPreset();
            
            // preset container
            _presetContainer = new VisualElement();
            _presetContainer.style.flexDirection = FlexDirection.Row;
            _presetContainer.style.flexGrow = 1;
            Add(_presetContainer);
            
            // preset selector
            _presetField = new CustomPresetField("Preset", variable, preset);
            _presetField.BindProperty(_presetProperty);
            _presetField.style.flexGrow = 1;
            _presetContainer.Add(_presetField);
            
            // create button
            _createPresetButton = new Button(CreatePreset)
            {
                text = "Create preset"
            };
            _presetContainer.Add(_createPresetButton);
            
            // editing foldout
            _editFoldout = new Foldout();
            _editFoldout.style.flexGrow = 1;
            _foldoutHeader = _editFoldout.Q(className: Foldout.inputUssClassName);
            Add(_editFoldout);
            _valueField = new InspectorElement(preset);
            _editFoldout.contentContainer.Add(_valueField);
            
            _presetField.RegisterValueChangedCallback(OnPresetChanged);
        }

        private void OnPresetChanged(ChangeEvent<Object> evt)
        {
            var newValue = evt.newValue;
            if (newValue is Preset preset)
            {
                UpdateUI(preset);
            }
            else
            {
                UpdateUI(null);
            }
        }

        private void OnPresetSelectedInSearchWindow(Preset selected)
        {
            _presetField.value = selected;
        }

        private void CreatePreset()
        {
            var parentDeployContext = EditorWindow.GetWindow<DeployEditorWindow>().CurrentContext;
            
            var variable = GetVariableObject();
            var path = AssetDatabase.GetAssetPath(variable);
            var directory = Path.GetDirectoryName(path);
            var newPresetPath = Path.Combine(directory, $"{variable.name}.{parentDeployContext.name}.asset");
            var preset = new Preset(variable);
            AssetDatabase.CreateAsset(preset, newPresetPath);

            _presetField.value = preset;
            EditorUtility.SetDirty(parentDeployContext);
        }

        private void UpdateUI(Preset selected)
        {
            if (_editFoldout.contentContainer.Contains(_valueField))
            {
                _editFoldout.contentContainer.Remove(_valueField);
                _valueField = null;
            }
            
            if (selected == null)
            {
                _createPresetButton.style.display = DisplayStyle.Flex;
                _editFoldout.style.display = DisplayStyle.None;
                
                Add(_presetContainer);
            }
            else
            {
                _createPresetButton.style.display = DisplayStyle.None;
                _editFoldout.style.display = DisplayStyle.Flex;
                
                _foldoutHeader.Add(_presetContainer);
                
                _valueField = new InspectorElement(selected);
                _editFoldout.contentContainer.Add(_valueField);
            }
        }

        private ScriptableObject GetVariableObject()
        {
            var variableObject = PropertiesUtils.GetTargetObjectOfProperty(_variableProperty) as ScriptableObject;
            return variableObject;
        }

        private Preset GetPreset()
        {
            var valueObject = PropertiesUtils.GetTargetObjectOfProperty(_presetProperty) as Preset;
            return valueObject;
        }

        public void OnVariableChange()
        {
            var variable = GetVariableObject();
            var preset = GetPreset();

            if (variable == null)
            {
                if (preset != null)  // remove preset because variable is null
                {
                    _presetField.value = null;
                }
            }
            else
            {
                if (preset != null)
                {
                    var variableType = variable.GetType();
                    var valueType = preset.GetType();
                    bool sameType = variableType == valueType;
                    if (!sameType)  // draw a new preset with the new type
                    {
                        _presetField.value = null;
                    }
                }
            }
        }
    }
    
    public class CustomPresetField : ObjectField
    {
        private static Texture PresetsIcon = EditorGUIUtility.IconContent("Preset.Context").image;

        private Object _target;
        
        public CustomPresetField(string label, Object target, Preset initialValue) : base(label)
        {
            _target = target;
            // Set the type of the objects that can be assigned
            objectType = typeof(Preset);
            value = initialValue;

            // get the default selector button and hide it
            var selector = this.Q(className: selectorUssClassName);
            selector.style.display = DisplayStyle.None;
            
            // Register a callback for the click event
            var selectButton = new Button();
            selectButton.ClearClassList();
            selectButton.AddToClassList(selectorUssClassName);
            selectButton.RegisterCallback<ClickEvent>(ShowPresetSelector);
            selector.parent.Add(selectButton);
        }

        private void ShowPresetSelector(ClickEvent evt)
        {
            var searchEntries = new List<SearchEntry<Preset>>
            {
                new ("None", null)
            };
            var presetsEntries = AssetDatabase.FindAssets("t:Preset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .Where(asset => asset is Preset)
                .Cast<Preset>()
                .Where(p => p.GetPresetType() == new PresetType(_target))
                .Select(preset => new SearchEntry<Preset>(preset.name, preset, PresetsIcon))
                .ToList();
            
            searchEntries.AddRange(presetsEntries);
            var position = EditorWindow.mouseOverWindow.position.min + (Vector2) evt.position;
            GenericSearchWindow<Preset>.Create(position, "Presets", searchEntries, OnSelected);
        }

        private void OnSelected(Preset selected)
        {
            value = selected;
        }
    }

    public class CustomPresetSelectionReceiver : PresetSelectorReceiver
    {
        public Action<Preset> OnSelected;
        public Action<Preset> OnClosed;
        
        public override void OnSelectionChanged(Preset selection)
        {
            OnSelected?.Invoke(selection);
        }

        public override void OnSelectionClosed(Preset selection)
        {
            OnClosed?.Invoke(selection);
        }
    }
}