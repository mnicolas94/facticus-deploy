using System;
using System.Collections.Generic;
using System.IO;
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
    public class PresetValueElement : VisualElement, IBuildVariableDrawer
    {
        private SerializedProperty _property;
        private SerializedProperty _variableProperty;
        private SerializedProperty _presetProperty;
        
        private VisualElement _presetContainer;
        private CustomPresetField _presetSelector;
        private Button _createPresetButton;
        
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
            _presetSelector = new CustomPresetField("Preset", variable, preset);
            _presetSelector.BindProperty(_presetProperty);
            _presetSelector.style.flexGrow = 1;
            _presetContainer.Add(_presetSelector);
            
            // create button
            _createPresetButton = new Button(CreatePresetAsset)
            {
                text = "Create preset"
            };
            _presetContainer.Add(_createPresetButton);
            
            _presetSelector.RegisterValueChangedCallback(OnPresetChanged);
        }

        private void UpdateUI(Preset selected)
        {
            if (selected == null)
            {
                _createPresetButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _createPresetButton.style.display = DisplayStyle.None;
            }
        }

        private void CreatePresetAsset()
        {
            var parentDeployContext = EditorWindow.GetWindow<DeployEditorWindow>().CurrentContext;
            
            var variable = GetVariableObject();
            var path = AssetDatabase.GetAssetPath(variable);
            var directory = Path.GetDirectoryName(path);
            var newPresetPath = Path.Combine(directory, $"{variable.name}.{parentDeployContext.name}.asset");
            var preset = new Preset(variable);
            AssetDatabase.CreateAsset(preset, newPresetPath);

            _presetSelector.value = preset;
            EditorUtility.SetDirty(parentDeployContext);
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

        public void OnVariableChange()
        {
            var variable = GetVariableObject();
            var preset = GetPreset();

            if (variable == null)
            {
                if (preset != null)  // remove preset because variable is null
                {
                    _presetSelector.value = null;
                }
            }
            else
            {
                if (preset != null)
                {
                    var variableTypeName = variable.GetType().FullName;
                    var valueTypeName = preset.GetPresetType().GetManagedTypeName();
                    bool sameType = variableTypeName == valueTypeName;
                    if (!sameType)  // draw a new preset with the new type
                    {
                        _presetSelector.value = null;
                    }
                }
            }
        }
    }
    
    public class CustomPresetField : ObjectField
    {
        private static readonly Texture PresetsIcon = EditorGUIUtility.IconContent("Preset.Context").image;

        private readonly PresetType _presetType;

        public CustomPresetField(string label, Object target, Preset initialValue) : base(label)
        {
            _presetType = new PresetType(target);
            
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
            
            // find preset and add as entries
            var presetsGuids = AssetDatabase.FindAssets("t:Preset");
            foreach (var presetGuid in presetsGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(presetGuid);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is Preset preset && preset.GetPresetType() == _presetType)
                    {
                        searchEntries.Add(new SearchEntry<Preset>(preset.name, preset, PresetsIcon));
                    }
                }
            }
            
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