using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deploy.Editor.BuildPlatforms;
using Deploy.Editor.Data;
using Deploy.Editor.DeployPlatforms;
using Deploy.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomEditor(typeof(BuildDeploySet))]
    [CanEditMultipleObjects]
    public class BuildDeploySetEditor : UnityEditor.Editor
    {
        private VisualElement _root;
        private Button _buildButton;
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            
            var tree = Resources.Load<VisualTreeAsset>("BuildDeploySetEditor");
            tree.CloneTree(_root);
            var defaultInspectorContainer = _root.Q("Default");
            InspectorElement.FillDefaultInspector(defaultInspectorContainer, serializedObject, this);

            _buildButton = _root.Q<Button>("BuildButton");
            _buildButton.clickable.clicked += OnBuildClicked;

            RegisterPlatformsEvents();
            RegisterOverrideVariablesEvents();

            return _root;
        }

        private async Task<ListView> GetListView(string propertyFieldName)
        {
            var cts = new CancellationTokenSource(1000);
            var ct = cts.Token;
            var variablesList = _root.Q<PropertyField>(propertyFieldName);

            if (variablesList == null)
            {
                return null;
            }

            while (!ct.IsCancellationRequested)
            {
                var list = variablesList.Q<ListView>();
                if (list != null)
                {
                    return list;
                }

                await Task.Yield();
            }

            return null;
        }
        
        private async void RegisterOverrideVariablesEvents()
        {
            var list = await GetListView("PropertyField:_variables");
            if (list == null)
            {
                return;
            }
            list.itemsAdded += OnVariableAdded;
            list.itemsRemoved += OnVariableRemoved;
        }
        
        private async void RegisterPlatformsEvents()
        {
            var list = await GetListView("PropertyField:_platforms");
            if (list == null)
            {
                return;
            }
            list.itemsAdded += OnPlatformsAdded;
        }

        private void OnPlatformsAdded(IEnumerable<int> newIndices)
        {
            var set = (BuildDeploySet) target;
            foreach (var i in newIndices)
            {
                var buildPlatform = set.Platforms[i].BuildPlatform;
                var deployPlatform = set.Platforms[i].DeployPlatform;
                set.Platforms[i].BuildPlatform = (IBuildPlatform) Clone(buildPlatform);
                set.Platforms[i].DeployPlatform = (IDeployPlatform) Clone(deployPlatform);
                
                EditorUtility.SetDirty(set);
            }
        }

        private object Clone(object original)
        {
            if (original != null)
            {
                var type = original.GetType();
                var json = JsonUtility.ToJson(original);
                var copy = JsonUtility.FromJson(json, type);
                return copy;
            }

            return null;
        }

        private void OnVariableAdded(IEnumerable<int> newIndices)
        {
            var set = (BuildDeploySet) target;
            foreach (var i in newIndices)
            {
                set.OverrideVariables[i].Value = null;
                set.OverrideVariables[i].Variable = null;
                EditorUtility.SetDirty(set);
            }
        }

        private void OnVariableRemoved(IEnumerable<int> indices)
        {
            var set = (BuildDeploySet) target;
            var values = set.OverrideVariables.Select(variable => variable.Value).ToList();

            var path = AssetDatabase.GetAssetPath(set);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var assetsToRemove = assets.Where(
                asset => AssetDatabase.IsSubAsset(asset) && !values.Contains(asset));
            foreach (var asset in assetsToRemove)
            {
                AssetDatabase.RemoveObjectFromAsset(asset);
            }
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
        }

        private async void OnBuildClicked()
        {
            try
            {
                _buildButton.SetEnabled(false);
                var backend = DeploySettings.GetOrCreate().Backend;
                var set = (BuildDeploySet) target;
                if (set.AllDisabled)
                {
                    EditorWindow.focusedWindow.ShowNotification(new GUIContent("Can't start workflow. All platforms are disabled"));
                }
                else
                {
                    var success = await backend.BuildAndDeploy(set);
                    if (success)
                    {
                        EditorWindow.focusedWindow.ShowNotification(new GUIContent("Workflow started successfully"));
                    }
                }
            }
            catch (Exception e)
            {
                EditorInputDialog.ShowMessage("Error", e.Message);
            }
            finally
            {
                _buildButton.SetEnabled(true);
            }
        }
    }
}