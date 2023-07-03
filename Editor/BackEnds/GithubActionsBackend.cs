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
using Deploy.Editor.Utility;
using UnityEngine;
using Utils.Editor;

namespace Deploy.Editor.BackEnds
{
    [Serializable]
    [AddTypeMenu("Github Actions")]
    public class GithubActionsBackend : ICicdBackend
    {
        public async Task<bool> BuildAndDeploy(BuildDeploySet set)
        {
            bool tokenIsNotValid = false;
            bool success;

            do
            {
                tokenIsNotValid = false;
                var elements = set.Platforms;
                var branch = set.RepositoryBranchOrTag;
                var buildSetInput = GetBuildSetInput(elements, set.OverrideVariables.ToList());
                var inputsString = $"{{\"json_parameters\":\"{buildSetInput}\"}}";

                var (owner, repo) = GetOwnerAndRepo();
                var workflowId = $"{DeploySettings.GetOrCreate().WorkflowId}.yml";
                var requestUri = $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflowId}/dispatches";
                var userAgent = GetGitUser();
                var entered = TryGetToken(out var token);
                success = entered;
                
                if (entered)
                {
                    var stringContent = $"{{\"ref\":\"{branch}\",\"inputs\":{inputsString}}}";
                    var requestContent = new StringContent(stringContent);
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), requestUri))
                        {
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
                            }
                            else if (responseCode >= 300)
                            {
                                throw new HttpRequestException($"Error {responseCode}\n{content}");
                            }
                        }
                    }
                }
            } while (tokenIsNotValid);
            
            return success;
        }
        
        public static string GetBuildSetInput(ReadOnlyCollection<BuildDeployElement> elements,
            List<BuildVariableValue> variables)
        {
            var inputStrings = elements.Select(element =>
            {
                var inputString = GetInputsString(element, variables);
                
                // prevent escaped double quotes to be affected by the next line
                inputString = inputString.Replace(@"\""", @"@@@");
                
                inputString = inputString.Replace("\"", @"\\\""");  // escape double quotes
                
                inputString = inputString.Replace("@@@", @"\\\\\\\""");

                return inputString;
            });

            var joined = string.Join(",", inputStrings);
            var buildSetInput = $"[{joined}]";
            return buildSetInput;
        }
        
        private static string GetInputsString(BuildDeployElement element, List<BuildVariableValue> variables)
        {
            var buildPlatform = element.BuildPlatform.GetGameCiName();
            var buildParameters = ToJson(element.BuildPlatform);
            var buildVariables = variables.OverrideVariablesToBase64();
            var developmentBuild = element.DevelopmentBuild;
            var freeDiskSpace = element.FreeDiskSpaceBeforeBuild;
            var deployPlatform = element.DeployPlatform.GetPlatformName();
            var deployParameters = ToJson(element.DeployPlatform);

            var notifyPlatform = DeploySettings.GetOrCreate().NotifyPlatform;
            var notifyPlatformName = notifyPlatform == null ? "" : notifyPlatform.GetPlatformName();
            
            var inputsString = "{" +
                               $"\"buildPlatform\":\"{buildPlatform}\"," +
                               $"\"buildParams\":\"{buildParameters}\"," +
                               $"\"buildVariables\":\"{buildVariables}\"," +
                               $"\"developmentBuild\":\"{developmentBuild.ToString().ToLower()}\"," +
                               $"\"freeDiskSpace\":\"{freeDiskSpace.ToString().ToLower()}\"," +
                               $"\"deployParams\":\"{deployParameters}\"," +
                               $"\"deployPlatform\":\"{deployPlatform}\"," +
                               $"\"notifyPlatform\":\"{notifyPlatformName}\"" +
                               "}";
            return inputsString;
        }

        private static string ToJson(object obj)
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
            json = PreProcessJsonString(json);
            
            return json;
        }

        private static string PreProcessJsonString(string buildParameters)
        {
            buildParameters = buildParameters.Replace("false", @"""false""");
            buildParameters = buildParameters.Replace("true", @"""true""");
            buildParameters = buildParameters.Replace("\"", "\\\"");
            return buildParameters;
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

        private static (string, string) GetOwnerAndRepo()
        {
            // limitations: only works on remotes named "origin"
            var (output, _) = TerminalUtils.RunCommand("git", "remote get-url origin", DeploySettings.GetOrCreate().GitDirectory);
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
    }
}