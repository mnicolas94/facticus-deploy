using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Deploy.Editor.BuildPlatforms;
using Deploy.Editor.Data;
using Deploy.Editor.Utility;
using Newtonsoft.Json;
using UnityBuilderAction;
using UnityEditor;
using UnityEngine;
using Utils.Attributes;

namespace Deploy.Editor.BackEnds
{
    [Serializable]
    [AddTypeMenu("Act (Locally)")]
    public class ActBackend : ICicdBackend
    {
        [SerializeField] private bool _buildWithAct;
        [SerializeField, PathSelector(isDirectory: true)] private string _buildsDirectory;
        [SerializeField, PathSelector] private string _secretsDir;
        
        public async Task<bool> BuildAndDeploy(BuildDeploySet set)
        {
            if (_buildWithAct)
            {
                BuildDeployWithAct(set);
            }
            else
            {
                await BuildLocallyDeployWithAct(set);
            }

            await Task.Yield();
            return true;
        }

        private void BuildDeployWithAct(BuildDeploySet set)
        {
            var elements = set.Platforms;
            var overrideVariables = set.OverrideVariables.ToList();
            
            var buildSetInput = GithubActionsBackend.GetBuildSetInput(elements, overrideVariables);
            
            var secretsPath = _secretsDir;
            secretsPath = Path.GetFullPath(secretsPath);
            var deploySettings = DeploySettings.GetOrCreate();
            var workflowName = $"{deploySettings.WorkflowId}.yml";
            var workflowPath = Path.Combine(".github", "workflows", workflowName);
            
            var command =
                $"workflow_dispatch -W {workflowPath}" +
                $" --input \"json_parameters={buildSetInput}\"" +
                $" --secret-file {secretsPath}";

            TerminalUtils.RunCommandMergeOutputs("act", command, deploySettings.GitDirectory, true);
            Debug.Log("Act started building. See outputs in terminal");
        }

        private async Task BuildLocallyDeployWithAct(BuildDeploySet set)
        {
            var elements = set.Platforms;
            var overrideVariables = set.OverrideVariables.ToList();
            
            for (int i = 0; i < elements.Count; i++)
            {
                var buildDeployElement = elements[i];
                
                // create build folder name
                var buildFolderName = $"{set.name}-{i}-{buildDeployElement.BuildPlatform.GetGameCiName()}";

                // build locally
                var buildPath = BuildLocally(buildDeployElement, buildFolderName, overrideVariables);
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
                            { "deployParams", buildDeployElement.DeployPlatform },
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

                // execute Act to deploy
                TerminalUtils.RunCommandMergeOutputs("act", command, DeploySettings.GetOrCreate().GitDirectory, true);
                Debug.Log("Act started deploying. See outputs in terminal");
            }
        }

        private string BuildLocally(BuildDeployElement element, string buildFolderName, List<BuildVariableValue> overrideVariables)
        {
            var projectPath = "./";
            var buildPlatform = element.BuildPlatform;
            var buildName = Application.productName;
            var fileName = buildName;
            
            // add extension based on target platform
            if (buildPlatform is Windows)
            {
                fileName += ".exe";
            }
            else if (buildPlatform is Android androidPlatform)
            {
                fileName += androidPlatform.AppBundle ? ".aab" : ".apk";
            }
            
            // construct build arguments
            var buildTarget = buildPlatform.GetGameCiName();
            var buildDir = Path.Combine(_buildsDirectory, buildFolderName, buildName);
            var buildPath = Path.Combine(buildDir, fileName);
            var version = Application.version;
            var androidVersionCode = GetAndroidVersionCode(version);
            var buildVariablesBase64 = overrideVariables.OverrideVariablesToBase64();
            var options = new Dictionary<string, string>
            {
                { "projectPath", projectPath },
                { "buildTarget", buildTarget },
                { "customBuildPath", buildPath },
                { "customBuildName", buildName },
                { "buildVersion", version },
                { "androidVersionCode", androidVersionCode },
                // mines
                { "buildVariables", buildVariablesBase64 },
            };
            
            // build
            BuildScript.Build(options, exit: false);
            
            if (buildPlatform is Windows or Linux or Mac or WebGL)  // zip build folder
            {
                var newBuildPath = $"{buildDir}.zip";
                if (File.Exists(newBuildPath))
                {
                    File.Delete(newBuildPath);
                }
                ZipFile.CreateFromDirectory(buildDir, newBuildPath);
                return newBuildPath;
            }
            else
            {
                return buildPath;
            }
        }

        private string GetAndroidVersionCode(string version)
        {
            var elements = version.Split('.');
            var major = elements[0];
            var minor = elements.Length >= 2 ? elements[1] : "000";
            var patch = elements.Length >= 3 ? elements[2] : "000";
            major = major.PadLeft(3, '0');
            minor = minor.PadLeft(3, '0');
            patch = patch.PadLeft(3, '0');
            return $"{major}{minor}{patch}";
        }

        private string GetDeployOnlyWorkflowFilePath()
        {
            var filePath = "Packages/com.facticus.deploy/.github/workflows/only_deploy.yml";
            if (!File.Exists(filePath))
            {
                // this means this project is my development project, should not happen for package's users
                filePath = "Assets/Deploy/.github/workflows/only_deploy.yml";
            }

            return filePath;
        }
    }
}