using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Deploy.Editor.Data;
using Deploy.Editor.DeployPlatforms;
using Deploy.Editor.Settings;
using Deploy.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utils.Editor;

namespace Deploy.Editor.BackEnds
{
    [Serializable]
    [AddTypeMenu("Github Actions")]
    public class GithubActionsBackend : ICicdBackend
    {
        public async Task<bool> BuildAndDeploy(DeployContext context)
        {
            // Display warning if workflow has changes
            if (ThereAreAnyChangesInWorkflow())
            {
                var message = $"Your workflow file ({GetWorkflowPath()}) has changes that have not been pushed to " + 
                              $"your remote repository. Are you sure you want to continue?";
                var doContinue = EditorInputDialog.ShowYesNoDialog("Warning", message);
                if (!doContinue)
                {
                    return false;
                }
            }
            
            bool tokenIsNotValid = false;
            bool success;

            do
            {
                tokenIsNotValid = false;
                var entered = TryGetToken(out var token);
                success = entered;

                if (!entered)
                {
                    continue;
                }
                
                // get repository data
                var branch = context.RepositoryBranchOrTag;
                var split = branch.Split('/');
                var remote = split.Length > 1 ? split[0] : "origin";
                
                branch = split.Length > 1 ? split[1] : branch;
                var (owner, repo) = GetOwnerAndRepo(remote);
                var workflowId = $"{DeploySettings.GetOrCreate().WorkflowId}.yml";
                var requestUri = $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflowId}/dispatches";
                var userAgent = GetGitUser();
                
                var inputs = GetGroupedWorkflowsInputs(context.Platforms, context.OverrideVariables.ToList());
                
                foreach (var input in inputs)
                {
                    var inputsString = $"{{\"json_parameters\":\"{input}\"}}";
                    var stringContent = $"{{\"ref\":\"{branch}\",\"inputs\":{inputsString}}}";
                        
                    var requestContent = new StringContent(stringContent);
                    using var httpClient = new HttpClient();
                    using var request = new HttpRequestMessage(new HttpMethod("POST"), requestUri);
                    request.Headers.TryAddWithoutValidation("User-Agent", $"{userAgent}");
                    request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
                    request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
                    request.Content = requestContent;
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    var responseCode = (int)response.StatusCode;

                    // Bad credentials
                    if (responseCode == 401)  // request token and try again
                    {
                        var message = "Your Github authentication token is no longer valid, please enter a new valid one.";
                        entered = TryAskForTokenDialog(message, out var _);
                        success = false;
                        if (entered)
                        {
                            tokenIsNotValid = true;  // try again
                        }

                        break;
                    }
                    else if (responseCode >= 300)
                    {
                        throw new HttpRequestException($"Error {responseCode}\n{content}");
                    }
                }
            } while (tokenIsNotValid);
            
            return success;
        }
        
        /// <summary>
        /// Group build-deploy elements by build platform to only build once if the same build platform
        /// is going to be deployed to several deploy platforms.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public static List<string> GetGroupedWorkflowsInputs(ReadOnlyCollection<BuildDeployElement> elements,
            List<BuildVariableValue> variables)
        {
            var inputStrings = elements
                .Where(element => element.Enabled)  // only build enabled elements
                .GroupBy(element =>  // group by build platforms with same parameters to only build once
                {
                    var platformName = element.BuildPlatform.GetGameCiName();
                    var parameters = ToJObject(element.BuildPlatform);
                    return $"{platformName}-{parameters}-{element.DevelopmentBuild}-{element.FreeDiskSpaceBeforeBuild}";
                })
                .Select(group =>
                {
                    var inputString = GetWorkflowInputAsBase64(group.ToList(), variables);
                    return inputString;
                });

            return inputStrings.ToList();
        }

        public static string GetWorkflowInputAsBase64(List<BuildDeployElement> elements,
            List<BuildVariableValue> variables)
        {
            var inputObject = GetWorkflowInputAsJsonObject(elements, variables);
            var json = inputObject.ToString(Formatting.None);
            
            // encode with base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var inputBase64 = Convert.ToBase64String(plainTextBytes);
            
            return inputBase64;
        }
        
        public static JObject GetWorkflowInputAsJsonObject(List<BuildDeployElement> elements, List<BuildVariableValue> variables)
        {
            var first = elements[0];
            var buildPlatform = first.BuildPlatform.GetGameCiName();
            var buildParameters = ToJObject(first.BuildPlatform);
            var overrideVariablesBase64 = variables.OverrideVariablesToBase64();
            var developmentBuild = first.DevelopmentBuild;
            var freeDiskSpace = first.FreeDiskSpaceBeforeBuild;
            
            var deploySettings = DeploySettings.GetOrCreate();
            var versioningStrategy = deploySettings.VersioningStrategy;
            var notifyPlatform = deploySettings.NotifyPlatform;
            var notifyPlatformName = notifyPlatform == null ? "" : notifyPlatform.GetPlatformName();

            // construct workflow input for each deploy platform
            var deployPlatformsList = elements.Select(e => new JObject
            {
                { "deployPlatform", e.DeployPlatform.GetPlatformName() },
                { "deployParams", AddDeployPlatformPrefixToFields(ToJObject(e.DeployPlatform), e.DeployPlatform) },
                { "notifyPlatform", notifyPlatformName },
            }).ToList();
            var deployPlatforms = new JArray(deployPlatformsList);

            var jsonObject = new JObject
            {
                { "buildPlatform", buildPlatform },
                { "buildParams", buildParameters },
                { "buildVariables", overrideVariablesBase64 },
                { "developmentBuild", developmentBuild },
                { "freeDiskSpace", freeDiskSpace },
                { "versioningStrategy", versioningStrategy.ToString() },
                { "deployPlatforms", deployPlatforms },
            };
            return jsonObject;
        }

        private static JObject ToJObject(object obj)
        {
            string json = "";
            if (obj is IJsonSerializable jsonSerializable)
            {
                json = jsonSerializable.ToJson();
            }
            else
            {
                json = JsonUtility.ToJson(obj);
            }

            return JObject.Parse(json);
        }

        private static JObject AddDeployPlatformPrefixToFields(JObject obj, IDeployPlatform platform)
        {
            var newObj = new JObject();
            foreach (var (key, value) in obj)
            {
                newObj.Add($"{platform.GetFieldsPrefix()}_{key}", value);
            }

            return newObj;
        }

        private static bool TryGetToken(out string token)
        {
            if (TryLoadToken(out token))
            {
                return true;
            }
            else
            {
                var entered = TryAskForTokenDialog(out token);
                return entered;
            }
        }

        private static bool TryAskForTokenDialog(string message, out string token)
        {
            var input = EditorInputDialog.ShowModal<TokenDialogInput>(
                "Enter token", message);
            
            bool entered = input != null;
            token = "";

            if (entered)
            {
                SaveToken(input.AuthToken);
                token = input.AuthToken;
            }
            
            return entered;
        }
        
        private static bool TryAskForTokenDialog(out string token)
        {
            var message = "Enter your github authentication token";
            return TryAskForTokenDialog(message, out token);
        }

        private static bool TryLoadToken(out string token)
        {
            try
            {
                var filePath = GetTokenPath();
                var file = File.Open(filePath, FileMode.Open);
        
                using var reader = new StreamReader(file);
                var json = reader.ReadToEnd();
                var settings = JsonUtility.FromJson<DeployPersistentSettings>(json);
                token = settings.GithubAuthToken;
                return true;
            }
            catch
            {
                token = "";
                return false;
            }
        }

        private static void SaveToken(string token)
        {
            var filePath = GetTokenPath();
            var file = File.Open(filePath, FileMode.Create);

            using var writer = new StreamWriter(file);
            var settings = new DeployPersistentSettings();
            settings.GithubAuthToken = token;
            var json = JsonUtility.ToJson(settings);
            writer.Write(json);
        }

        private static string GetTokenPath()
        {
            var tokenPath = Path.Combine(Application.persistentDataPath, "deploy_settings.data");
            return tokenPath;
        }

        private static (string, string) GetOwnerAndRepo(string remote = "origin")
        {
            // limitations: only works on remotes named "origin"
            var (output, _) = TerminalUtils.RunCommand("git", $"remote get-url {remote}", DeploySettings.GetOrCreate().GitDirectory);
            var rx = new Regex("https://github.com/(.*)/(.*).git");
            var matches = rx.Matches(output);
            var owner = matches[0].Groups[1].Value;
            var repo = matches[0].Groups[2].Value;
            return (owner, repo);
        }

        private static string GetGitUser()
        {
            string gitCommand = "config --get user.name";
            var (stdout, _) = TerminalUtils.RunCommand("git",gitCommand, DeploySettings.GetOrCreate().GitDirectory);
            stdout = stdout.Trim();
            return stdout;
        }

        private static bool ThereAreAnyChangesInWorkflow()
        {
            var deploySettings = DeploySettings.GetOrCreate();
            var gitRoot = deploySettings.GitDirectory;

            try
            {
                // this is needed in some cases to allow "diff-index" command bellow to work properly
                TerminalUtils.RunCommand("git","update-index --refresh", gitRoot);
            }
            catch
            {
                // ignored
            }

            var workflowPath = GetWorkflowPath();
            var relativePath = MakePathRelativeToRoot(workflowPath, gitRoot);
            relativePath = relativePath.Trim('\\', '/');
            var (output, error) = TerminalUtils.RunCommand("git", $"diff-index HEAD -- {relativePath}", gitRoot);
            if (!String.IsNullOrEmpty(output) || !String.IsNullOrEmpty(error))
            {
                // there is a change
                return true;
            }
            
            return false;
        }

        private static string GetWorkflowPath()
        {
            var deploySettings = DeploySettings.GetOrCreate();
            var gitRoot = deploySettings.GitDirectory;
            return Path.Combine(gitRoot, ".github", "workflows", $"{deploySettings.WorkflowId}.yml");
        }

        private static string MakePathRelativeToRoot(string workPath, string gitRoot)
        {
            if (gitRoot == "")
            {
                return workPath;
            }
            return workPath.Replace(gitRoot, "");
        }
    }
}