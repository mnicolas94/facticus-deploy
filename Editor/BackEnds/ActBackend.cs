using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Deploy.Editor.Data;
using Deploy.Editor.Utility;
using Newtonsoft.Json;
using UnityBuilderAction;
using UnityEditor;
using UnityEngine;

namespace Deploy.Editor.BackEnds
{
    [Serializable]
    [AddTypeMenu("Act (Locally)")]
    public class ActBackend : ICicdBackend
    {
        [SerializeField] private bool _buildWithAct;
        [SerializeField] private string _buildsDirectory;
        [SerializeField] private string _secretsDir;
        
        public async Task<bool> BuildAndDeploy(BuildDeploySet set)
        {
            var elements = set.Platforms;

            var command = "";
            if (_buildWithAct)
            {
                var buildSetInput = GithubActionsBackend.GetBuildSetInput(elements, set.OverrideVariables.ToList());
                var workflowName = "build_and_deploy";
                command =
                    $"workflow_dispatch -W .\\.github\\workflows\\{workflowName}.yml" +
                    $" --input \"json_parameters={buildSetInput}\"" +
                    $" --secret-file .\\_extras\\my.secrets";

                Debug.Log("Act started building. See outputs in terminal");
            }
            else
            {
                // todo foreach build-deploy element
                //  get buildPlatform
                var buildPath = BuildLocally(set);
                var workflowName = "only_deploy";
                var inputs = new Dictionary<string, object>
                {
                    { "inputs", new Dictionary<string, object>
                    {
                        { "buildPath", buildPath },
                        { "version", Application.version },
                        { "deployPlatform", "Telegram" },
                        { "deployParams", new Dictionary<string, string>{ { "message", "Asd123"}} },
                        { "buildPlatform", "StandaloneWindows64" },
                        { "notifyPlatform", "Telegram" },
                    }}
                };
                
                // write json
                var json = JsonConvert.SerializeObject(inputs, Formatting.Indented);
                var jsonDirectory = "Temp/Deploy/";
                Directory.CreateDirectory(jsonDirectory);
                var jsonPath = Path.Combine(jsonDirectory, "actPayload.json");
                jsonPath = Path.GetFullPath(jsonPath);
                File.WriteAllText(jsonPath, json);
                
                var secretsPath = "_extras\\my.secrets";
                secretsPath = Path.GetFullPath(secretsPath);

                command =
                    $"workflow_dispatch -W .\\Assets\\Deploy\\.github\\workflows\\{workflowName}.yml" +
                    $" -e {jsonPath}" +
                    $" -b" +
                    $" -C ..\\..\\" +
                    $" --secret-file {secretsPath}";
                
                Debug.Log("Act started deploying. See outputs in terminal");
            }
            TerminalUtils.RunCommandMergeOutputs("act", command, DeploySettings.GetOrCreate().GitDirectory, true);

            await Task.Yield();
            return true;
        }
        
        private IEnumerable<string> BuildLocally(BuildDeploySet set)
        {
            foreach (var buildDeployElement in set.Platforms)
            {
                var buildPlatform = buildDeployElement.BuildPlatform.GetGameCiName();
                var deployPlatform = buildDeployElement.DeployPlatform.GetPlatformName();
                var projectPath = "./";
                var buildTarget = "StandaloneWindows64";
                var buildName = Application.productName;
                var buildDir = Path.Combine(_buildsDirectory, "builddeployelement number", buildName);
                var fileName = $"{buildName}.exe";  // poner en la carpeta
                var buildPath = Path.Combine(buildDir, fileName);
                var version = Application.version;
                var androidVersionCode = GetAndroidVersionCode(version);
                var buildVariablesBase64 = set.OverrideVariables.ToList().OverrideVariablesToBase64();
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
                BuildScript.Build(options, exit: false);
                
                // todo
                // ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath);

                yield return buildPath;
            }
        }

        private static string GetAndroidVersionCode(string version)
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

        [MenuItem("Debug/Zip")]
        public static void Zip()
        {
            string sourceFolderPath = "_builds/StandaloneWindows64";
            string zipFilePath = "_builds/StandaloneWindows64.zip";
            ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath);
            Debug.Log("zipped");
        }
    }
}