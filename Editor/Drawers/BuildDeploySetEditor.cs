using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deploy.Editor.Drawers
{
    [CustomEditor(typeof(BuildDeploySet))]
    [CanEditMultipleObjects]
    public class BuildDeploySetEditor : UnityEditor.Editor
    {
        private Button _buildButton;
        private PropertyField _variablesList;
        private ListView _list;
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var tree = Resources.Load<VisualTreeAsset>("BuildDeploySetEditor");
            tree.CloneTree(root);
            var defaultInspectorContainer = root.Q("Default");
            InspectorElement.FillDefaultInspector(defaultInspectorContainer, serializedObject, this);

            _buildButton = root.Q<Button>("BuildButton");
            _buildButton.clickable.clicked += OnBuildClicked;

            _variablesList = root.Q<PropertyField>("PropertyField:_variables");
            
            GetList();

            return root;
        }

        private async void GetList()
        {
            var cts = new CancellationTokenSource(1000);
            var ct = cts.Token;
            bool found = false;
            
            while (!found && !ct.IsCancellationRequested)
            {
                var list = _variablesList.Q<ListView>();
                if (list != null)
                {
                    found = true;
                    _list = list;

                    _list.itemsAdded += OnVariableAdded;
                    _list.itemsRemoved += OnVariableRemoved;
                }

                await Task.Yield();
            }
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
                var success = await BuildDeploy.BuildAndDeploySet((BuildDeploySet) target);
                if (success)
                {
                    EditorWindow.focusedWindow.ShowNotification(new GUIContent("Workflow started successfully"));
                }
            }
            finally
            {
                _buildButton.SetEnabled(true);
            }
        }
    }
}