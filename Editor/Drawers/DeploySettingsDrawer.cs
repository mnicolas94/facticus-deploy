using System;
using System.Collections.Generic;
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
            var drawers = new Dictionary<string, Func<SerializedProperty, VisualElement>>
            {
                { DeploySettings.PropertyNameWorkflowId, DrawWorkflowIdField },
            };
            
            var sps = PropertiesUtils.GetSerializedProperties(serializedObject);
            foreach (var serializedProperty in sps)
            {
                var spName = serializedProperty.name;
                VisualElement visualElement;
                if (drawers.ContainsKey(spName))
                {
                    visualElement = drawers[spName](serializedProperty);
                }
                else
                {
                    visualElement = new PropertyField(serializedProperty);
                }
                _root.Add(visualElement);
            }
            
            return _root;
        }

        private VisualElement DrawWorkflowIdField(SerializedProperty sp)
        {
            var workflowIdField = new PropertyField(sp)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            var generateWorkflowButton = new Button(OnGenerateWorkflow)
            {
                text = "Generate workflow"
            };
            var workflowContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            workflowContainer.Add(workflowIdField);
            workflowContainer.Add(generateWorkflowButton);
            return workflowContainer;
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