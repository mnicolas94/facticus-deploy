using System.IO;
using Deploy.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
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
            var gitDirectoryField = new PropertyField(serializedObject.FindProperty("_gitDirectory"));
            var defaultAssetDirectoryField = new PropertyField(serializedObject.FindProperty("_defaultAssetDirectory"));
            var backendField = new PropertyField(serializedObject.FindProperty("_backend"));
            var notifyPlatformField = new PropertyField(serializedObject.FindProperty("_notifyPlatform"));
            var versioningStrategyField = new PropertyField(serializedObject.FindProperty("_versioningStrategy"));

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
            _root.Add(gitDirectoryField);
            _root.Add(defaultAssetDirectoryField);
            _root.Add(backendField);
            _root.Add(notifyPlatformField);
            _root.Add(versioningStrategyField);
            return _root;
        }

        private void OnGenerateWorkflow()
        {
            // get workflow name
            var dialogOutput = EditorInputDialog.ShowModal<StringContainer>(
                "Generate workflow",
                "Enter the workflow name"
            );
            var entered = dialogOutput != null;
            if (!entered)
            {
                return;
            }
            
            var workflowName = dialogOutput.Value;
            var fileNameWithExt = $"{workflowName}.yml";
            
            var templatePath = GetTemplatePath();
            
            // create workflows directory if it does not exist
            var gitDirectory = DeploySettings.Instance.GitDirectory;
            var destinyDir = Path.Combine(gitDirectory, ".github", "workflows");
            Directory.CreateDirectory(destinyDir);
            
            // copy the workflow file
            var destinyPath = Path.Combine(destinyDir, fileNameWithExt);
            bool copy = true;
            if (File.Exists(destinyPath))
            {
                var yes = EditorInputDialog.ShowYesNoDialog(
                    "Workflow file exists",
                    "A workflow file with that name already exists. Do you want to overwrite it?"
                );
                copy = yes;
                if (yes)
                {
                    File.Delete(destinyPath);
                }
            }

            if (copy)
            {
                File.Copy(templatePath, destinyPath);
                DeploySettings.GetOrCreate().WorkflowId = workflowName;
            }
        }

        private string GetTemplatePath()
        {
            var templateFileName = "workflow_template.yml";
            var packageDir = "Packages/com.facticus.deploy/Editor/Resources";

            var filePath = Path.Combine(packageDir, templateFileName);

            if (!File.Exists(filePath))
            {
                // this means this project is my development project, should not happen for package's users
                packageDir = "Assets/Deploy/Editor/Resources";
                filePath = Path.Combine(packageDir, templateFileName);
            }

            return filePath;
        }
    }
}