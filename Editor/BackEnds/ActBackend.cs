﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Deploy.Editor.BuildPlatforms;
using Deploy.Editor.Data;
using Deploy.Editor.Settings;
using Deploy.Editor.Utility;
using Newtonsoft.Json;
using UnityBuilderAction;
using UnityEngine;
using Utils.Attributes;

namespace Deploy.Editor.BackEnds
{
    [Serializable]
    [AddTypeMenu("Act (Locally)")]
    public class ActBackend : ICicdBackend
    {
        [SerializeField] private bool _buildWithAct;
        [SerializeField, Tooltip("The console output will give more details")]
        private bool _verboseOutput;

        [SerializeField] private List<string> _extraCommands;
        
        [SerializeField, PathSelector(isDirectory: true)] private string _buildsDirectory;
        [SerializeField, PathSelector] private string _secretsDir;
        
        public async Task<bool> BuildAndDeploy(DeployContext context)
        {
            if (_buildWithAct)
            {
                BuildDeployWithAct(context);
            }
            else
            {
                await BuildLocallyDeployWithAct(context);
            }

            await Task.Yield();
            return true;
        }

        private void BuildDeployWithAct(DeployContext context)
        {
            var elements = context.Platforms;
            var overrideVariables = context.OverrideVariables.ToList();
            
            var inputs = GithubActionsBackend.GetWorkflowInputAsJson(elements, overrideVariables);
            var base64Input = GithubActionsBackend.ConvertJsonObjectToBase64(inputs);
            var secretsPath = _secretsDir;
            secretsPath = Path.GetFullPath(secretsPath);
            var deploySettings = DeploySettings.GetOrCreate();
            var workflowName = $"{deploySettings.WorkflowId}.yml";
            var workflowPath = Path.Combine(".github", "workflows", workflowName);
        
            var command =
                $"workflow_dispatch -W {workflowPath}" +
                $" --input \"base64_json={base64Input}\"" +
                $" --secret-file {secretsPath}";

            command = AddExtraArgumentsToActCommand(command);

            TerminalUtils.RunCommandMergeOutputs("act", command, deploySettings.GitDirectory, true);
            Debug.Log("Act started building. See outputs in terminal");
        }

        private async Task BuildLocallyDeployWithAct(DeployContext context)
        {
            var elements = context.Platforms;
            var overrideVariables = context.OverrideVariables.ToList();
            
            for (int i = 0; i < elements.Count; i++)
            {
                var buildDeployElement = elements[i];
                
                // create build folder name
                var buildFolderName = $"{context.name}-{i}-{buildDeployElement.BuildPlatform.GetGameCiName()}";

                // build locally
                var buildPath = Utility.BuildLocally(buildDeployElement, _buildsDirectory, buildFolderName, overrideVariables);
                buildPath = buildPath.Replace("\\", "/");

                var workflowPath = GetDeployOnlyWorkflowFilePath();

                // construct payload inputs
                var inputs = new Dictionary<string, object>
                {
                    {
                        "inputs", new Dictionary<string, object>
                        {
                            { "buildPath", buildPath },
                            { "version", Application.version },
                            { "deployPlatform", buildDeployElement.DeployPlatform.GetPlatformName() },
                            { "deployParams", JsonUtility.ToJson(buildDeployElement.DeployPlatform) },
                            { "buildPlatform", buildDeployElement.BuildPlatform.GetGameCiName() },
                            { "notifyPlatform", DeploySettings.GetOrCreate().NotifyPlatform.GetPlatformName() },
                        }
                    }
                };

                // write json
                var json = JsonConvert.SerializeObject(inputs, Formatting.Indented);
                var jsonDirectory = "Temp/Deploy/";
                Directory.CreateDirectory(jsonDirectory);
                var jsonPath = Path.Combine(jsonDirectory, "actPayload.json");
                jsonPath = Path.GetFullPath(jsonPath);
                await File.WriteAllTextAsync(jsonPath, json);

                var secretsPath = _secretsDir;
                secretsPath = Path.GetFullPath(secretsPath);

                // construct command
                var workingDir = Path.GetFullPath("./");
                var command =
                    $"workflow_dispatch -W {workflowPath}" +
                    $" -e {jsonPath}" +
                    $" -b" +
                    $" -C {workingDir}" +
                    $" --secret-file {secretsPath}";

                command = AddExtraArgumentsToActCommand(command);
                
                // execute Act to deploy
                TerminalUtils.RunCommandMergeOutputs("act", command, DeploySettings.GetOrCreate().GitDirectory, true);
                Debug.Log("Act started deploying. See outputs in terminal");
            }
        }

        private static string GetDeployOnlyWorkflowFilePath()
        {
            var filePath = "Packages/com.facticus.deploy/.github/workflows/only_deploy.yml";
            if (!File.Exists(filePath))
            {
                // this means this project is my development project, should not happen for package's users
                filePath = "Assets/Deploy/.github/workflows/only_deploy.yml";
            }
            else
            {
                // copy the workflow to a temporary path
                var tempDir = "Temp/Deploy";
                Directory.CreateDirectory(tempDir);
                var tempPath = Path.Combine(tempDir, "only_deploy.yml");

                if (!File.Exists(tempPath))
                {
                    File.Copy(filePath, tempPath);
                }
                filePath = tempPath;
            }

            return filePath;
        }

        private string AddExtraArgumentsToActCommand(string command)
        {
            if (_verboseOutput)
            {
                command += " -v";
            }

            foreach (var extraCommand in _extraCommands)
            {
                command += $" {extraCommand}";
            }
            
            return command;
        }
        
        private Dictionary<string, object> ToDict(object obj)
        {
            var json = JsonUtility.ToJson(obj);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dict;
        }
    }
}