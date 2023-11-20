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
    [CustomEditor(typeof(DeployContext))]
    [CanEditMultipleObjects]
    public class DeployContextEditor : UnityEditor.Editor
    {
        private VisualElement _root;
        private Button _buildButton;
        private Button _buildLocallyButton;
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            
            var tree = Resources.Load<VisualTreeAsset>("DeployContextEditor");
            tree.CloneTree(_root);
            var defaultInspectorContainer = _root.Q("Default");
            InspectorElement.FillDefaultInspector(defaultInspectorContainer, serializedObject, this);

            _buildButton = _root.Q<Button>("BuildButton");
            _buildButton.clickable.clicked += OnBuildClicked;
            
            _buildLocallyButton = _root.Q<Button>("BuildLocallyButton");
            _buildLocallyButton.clickable.clicked += OnBuildLocallyClicked;

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
            var context = (DeployContext) target;
            foreach (var i in newIndices)
            {
                var buildPlatform = context.Platforms[i].BuildPlatform;
                var deployPlatform = context.Platforms[i].DeployPlatform;
                context.Platforms[i].BuildPlatform = (IBuildPlatform) Clone(buildPlatform);
                context.Platforms[i].DeployPlatform = (IDeployPlatform) Clone(deployPlatform);
                
                EditorUtility.SetDirty(context);
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
            var context = (DeployContext) target;
            foreach (var i in newIndices)
            {
                context.OverrideVariables[i].Value = null;
                context.OverrideVariables[i].Variable = null;
                EditorUtility.SetDirty(context);
            }
        }

        private void OnVariableRemoved(IEnumerable<int> indices)
        {
            var context = (DeployContext) target;
            var values = context.OverrideVariables.Select(variable => variable.Value).ToList();

            var path = AssetDatabase.GetAssetPath(context);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var assetsToRemove = assets.Where(
                asset => AssetDatabase.IsSubAsset(asset) && !values.Contains(asset));
            foreach (var asset in assetsToRemove)
            {
                AssetDatabase.RemoveObjectFromAsset(asset);
            }
            EditorUtility.SetDirty(context);
            AssetDatabase.SaveAssets();
        }

        private async void OnBuildClicked()
        {
            try
            {
                _buildButton.SetEnabled(false);
                var backend = DeploySettings.GetOrCreate().Backend;
                var context = (DeployContext) target;
                if (context.AllDisabled)
                {
                    EditorWindow.focusedWindow.ShowNotification(new GUIContent("Can't start workflow. All platforms are disabled"));
                }
                else
                {
                    var success = await backend.BuildAndDeploy(context);
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

        private void OnBuildLocallyClicked()
        {
            try
            {
                var context = (DeployContext)target;
                if (context.AllDisabled)
                {
                    EditorWindow.focusedWindow.ShowNotification(
                        new GUIContent("Can't start workflow. All platforms are disabled"));
                }
                else
                {
                    BackEnds.Utility.BuildLocally(context);
                }
            }
            catch (Exception e)
            {
                EditorInputDialog.ShowMessage("Error", e.Message);
                throw;
            }
        }
    }
}