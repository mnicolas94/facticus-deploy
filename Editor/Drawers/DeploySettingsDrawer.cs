using System.IO;
using Deploy.Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Deploy.Editor.Drawers
{
    [CustomEditor(typeof(DeploySettings))]
    public class DeploySettingsDrawer : UnityEditor.Editor
    {
        private VisualElement _root;
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            
            var workflowIdField = new PropertyField(serializedObject.FindProperty("_workflowId"));
            var defaultBranchField = new PropertyField(serializedObject.FindProperty("_defaultBranch"));
            var defaultAssetDirectoryField = new PropertyField(serializedObject.FindProperty("_defaultAssetDirectory"));

            workflowIdField.style.flexGrow = 1;
            var workflowContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            var generateWorkflowButton = new Button(OnGenerateWorkflow)
            {
                text = "Generate workflow"
            };
            workflowContainer.Add(workflowIdField);
            workflowContainer.Add(generateWorkflowButton);
            
            _root.Add(workflowContainer);
            _root.Add(defaultBranchField);
            _root.Add(defaultAssetDirectoryField);
            return _root;
        }

        private void OnGenerateWorkflow()
        {
            // get workflow name
            var workflowName = EditorInputDialog.ShowModal<StringContainer>(
                "Generate workflow",
                "Enter the workflow name"
            ).Value;
            var fileNameWithExt = $"{workflowName}.yml";
            
            var templatePath = GetTemplatePath();
            
            // create workflows directory if it does not exist
            var destinyDir = ".github/workflows";
            Directory.CreateDirectory(destinyDir);
            
            // copy the workflow file
            var destinyPath = Path.Join(destinyDir, fileNameWithExt);
            File.Copy(templatePath, destinyPath);

            DeploySettings.GetOrCreate().WorkflowId = workflowName;
        }

        private string GetTemplatePath()
        {
            var templateFileName = "workflow_template.yml";
            var packageDir = "Packages/com.facticus.deploy/Editor/Resources";

            var filePath = Path.Join(packageDir, templateFileName);

            if (!File.Exists(filePath))
            {
                // this means this project is my development project, should not happen for package's users
                packageDir = "Assets/Deploy/Editor/Resources";
                filePath = Path.Join(packageDir, templateFileName);
            }

            return filePath;
        }
    }
}