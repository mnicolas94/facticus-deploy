using System;
using Deploy.Editor.Data;
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
        private Button _buildButton;
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var tree = Resources.Load<VisualTreeAsset>("BuildDeploySetEditor");
            tree.CloneTree(root);
            var defaultInspectorContainer = root.Q("Default");
            InspectorElement.FillDefaultInspector(defaultInspectorContainer, serializedObject, this);

            _buildButton = root.Q<Button>("BuildButton");
            _buildButton.clickable.clicked += OnBuildClicked;
            
            return root;
        }

        private async void OnBuildClicked()
        {
            bool error = false;
            string response = "";
            try
            {
                _buildButton.SetEnabled(false);
                response = await ((BuildDeploySet)target).Build();
                _buildButton.SetEnabled(true);
            }
            catch (Exception e)
            {
                error = true;
                response = e.Message;
            }

            response = string.IsNullOrEmpty(response) ? "Workflow started succesfully" : response;
            if (error)
            {
                EditorInputDialog.ShowMessage("Error", response);
                
            }
            else
            {
                EditorWindow.focusedWindow.ShowNotification(new GUIContent(response));
            }
        }
    }
}