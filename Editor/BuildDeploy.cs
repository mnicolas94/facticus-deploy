using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Deploy.Editor.Data;
using Deploy.Editor.Utility;
using UnityEditor;
using UnityEngine;
using Utils.Editor;
using Debug = UnityEngine.Debug;

namespace Deploy.Editor
{
    public static class BuildDeploy
    {
        public static async Task<bool> BuildAndDeploySet(BuildDeploySet set)
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
                                EditorInputDialog.ShowMessage($"Error {responseCode}", content);
                                success = false;
                            }
                        }
                    }
                }
            } while (tokenIsNotValid);
            
            return success;
        }

        public static void BuildAndDeploySetLocally(BuildDeploySet set)
        {
            var elements = set.Platforms;

            var buildSetInput = GetBuildSetInput(elements, set.OverrideVariables.ToList());

            var workflowName = "build_and_deploy";
            // var workflowName = "test";
            var command =
                $"workflow_dispatch -W .\\.github\\workflows\\{workflowName}.yml" +
                $" --input \"json_parameters={buildSetInput}\"" +
                $" --secret-file .\\_extras\\my.secrets";

            var output = RunCommandMergeOutputs("act", command, DeploySettings.GetOrCreate().GitDirectory, true);
            Debug.Log(output);
        }

        public static void ProcessBuildVariables(string encodedVariables)
        {
            var variables = VariablesFromBase64(encodedVariables);
            
            foreach (var serializableVariable in variables)
            {
                var guid = serializableVariable.VariableGuid;
                var valueJson = serializableVariable.ValueJson;

                var variablePath = AssetDatabase.GUIDToAssetPath(guid);
                var variable = AssetDatabase.LoadMainAssetAtPath(variablePath);
                JsonUtility.FromJsonOverwrite(valueJson, variable);
                EditorUtility.SetDirty(variable);
            }
            
            AssetDatabase.SaveAssets();
        }
        
        private static string GetBuildSetInput(ReadOnlyCollection<BuildDeployElement> elements,
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
            var buildVariables = VariablesToBase64(variables);
            var developmentBuild = element.DevelopmentBuild;
            var deployPlatform = element.DeployPlatform.GetPlatformName();
            var deployParameters = ToJson(element.DeployPlatform);

            var notifyPlatform = DeploySettings.GetOrCreate().NotifyPlatform;
            var notifyPlatformName = notifyPlatform == null ? "" : notifyPlatform.GetPlatformName();
            
            var inputsString = "{" +
                               $"\"buildPlatform\":\"{buildPlatform}\"," +
                               $"\"buildParams\":\"{buildParameters}\"," +
                               $"\"buildVariables\":\"{buildVariables}\"," +
                               $"\"developmentBuild\":\"{developmentBuild.ToString().ToLower()}\"," +
                               $"\"deployParams\":\"{deployParameters}\"," +
                               $"\"deployPlatform\":\"{deployPlatform}\"," +
                               $"\"notifyPlatform\":\"{notifyPlatformName}\"" +
                               "}";
            return inputsString;
        }

        private static string VariablesToBase64(List<BuildVariableValue> variables)
        {
            var serializableVariables = variables.ConvertAll(variableValue =>
            {
                var variable = variableValue.Variable;
                var value = variableValue.Value;
                var path = AssetDatabase.GetAssetPath(variable);
                var guid = AssetDatabase.AssetPathToGUID(path);
                var valueJson = JsonUtility.ToJson(value);
                var serializable = new BuildVariableValueJsonSerializable(guid, valueJson);
                return serializable;
            });
            var serializableList = new BuildVariableValueJsonSerializableList(serializableVariables);
            
            var json = JsonUtility.ToJson(serializableList);
            
            // encode with base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(plainTextBytes);
            
            return base64;
        }

        private static List<BuildVariableValueJsonSerializable> VariablesFromBase64(string base64Encoded)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64Encoded);
            var buildVariables = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            var serializableVariables = JsonUtility.FromJson<BuildVariableValueJsonSerializableList>(buildVariables);

            return serializableVariables.SerializedVariables;
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
            var (output, _) = RunCommand("git", "remote get-url origin", DeploySettings.GetOrCreate().GitDirectory);
            var rx = new Regex("https://github.com/(.*)/(.*).git");
            var matches = rx.Matches(output);
            var owner = matches[0].Groups[1].Value;
            var repo = matches[0].Groups[2].Value;
            return (owner, repo);
        }

        private static string GetGitUser()
        {
            string gitCommand = "config --get user.name";
            var (stdout, _) = RunCommand("git",gitCommand, DeploySettings.GetOrCreate().GitDirectory);
            stdout = stdout.Trim();
            return stdout;
        }

        private static (string, string) RunCommand(string command, string options, string workingDir,
            bool createAsCmdPopup = false)
        {
            if (createAsCmdPopup)
            {
                options = $"/k {command} {options}";
                command = "cmd.exe";
            }
            
            // Set up our processInfo to run the command and log to output and errorOutput.
            ProcessStartInfo processInfo = new ProcessStartInfo(command, options)
            {
                WorkingDirectory = workingDir,
                CreateNoWindow = !createAsCmdPopup, // We want no visible pop-ups
                UseShellExecute = createAsCmdPopup, // Allows us to redirect input, output and error streams
                RedirectStandardOutput = !createAsCmdPopup, // Allows us to read the output stream
                RedirectStandardError = !createAsCmdPopup // Allows us to read the error stream
            };

            // Set up the Process
            Process process = new Process
            {
                StartInfo = processInfo
            };
            process.Start();

            if (createAsCmdPopup)
            {
                return ("", "");
            }
            
            // Read the results back from the process so we can get the output and check for errors
            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();  // Make sure we wait till the process has fully finished.
            int exitCode = process.ExitCode;
            bool hadErrors = process.ExitCode != 0;
            process.Close();        // Close the process ensuring it frees it resources.

            if (hadErrors)
            {
                Debug.Log($"Exit code: {exitCode}");    
                throw new Exception($"{output}\n{errorOutput}");
            }

            return (output, errorOutput);
        }
        
        private static string RunCommandMergeOutputs(string command, string options, string workingDir, bool createNoWindow=false)
        {
            var (output, errorOutput) = RunCommand(command, options, workingDir, createNoWindow);

            return $"Output:\n{output}\n\nErrorOutput:\n{errorOutput}";  // Return the output
        }
    }
}