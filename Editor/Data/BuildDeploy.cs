using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Deploy.Editor.Utility;
using UnityEngine;
using Utils.Editor;
using Debug = UnityEngine.Debug;

namespace Deploy.Editor.Data
{
    public static class BuildDeploy
    {
        public static async Task<string> BuildAndDeploySet(BuildDeploySet set)
        {
            var elements = set.Elements;
            var branch = set.RepositoryBranch;
            var buildSetInput = GetBuildSetInput(elements);
            var inputsString = $"{{\"build_set\":\"{buildSetInput}\"}}";
            
            var (owner, repo) = GetOwnerAndRepo();
            var workflowId = $"{DeploySettings.GetOrCreate().WorkflowId}.yml";
            var requestUri = $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflowId}/dispatches";
            var userAgent = GetGitUser();
            var token = GetToken();
            
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
                    var responseCode = (int) response.StatusCode;
                    if (responseCode >= 300)
                    {
                        throw new HttpRequestException(content);
                    }

                    return content;
                }
            }
        }
        
        public static void BuildAndDeploySetLocally(BuildDeploySet set)
        {
            var elements = set.Elements;

            var buildSetInput = GetBuildSetInput(elements);

            var command =
                $"act workflow_dispatch -W .\\.github\\workflows\\build_and_deploy_test.yml" +
                $" --input \"build_set={buildSetInput}\"";

            var output = RunCommandMergeOutputs("gh", command);
            Debug.Log(output);
        }

        private static string GetBuildSetInput(ReadOnlyCollection<BuildDeployElement> elements)
        {
            var inputStrings = elements.Select(element =>
            {
                var inputString = GetInputsString(element);
                
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
        
        private static string GetInputsString(BuildDeployElement element)
        {
            var buildPlatform = element.BuildPlatform.GameCiName;
            var buildParameters = ToJson(element.BuildPlatform);
            var developmentBuild = element.DevelopmentBuild;
            var deployPlatform = element.DeployPlatform.PlatformName;
            var deployParameters = ToJson(element.DeployPlatform);
            var inputsString = "{" +
                               $"\"buildPlatform\":\"{buildPlatform}\"," +
                               $"\"buildParams\":\"{buildParameters}\"," +
                               $"\"developmentBuild\":\"{developmentBuild.ToString().ToLower()}\"," +
                               $"\"deployParams\":\"{deployParameters}\"," +
                               $"\"deployPlatform\":\"{deployPlatform}\"" +
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

        private static string GetToken()
        {
            if (TryLoadToken(out var token))
            {
                return token;
            }
            else
            {
                var input = EditorInputDialog.ShowModal<TokenDialogueInput>(
                    "Enter token", "Enter your github authentication token");
                SaveToken(input.AuthToken);
                return input.AuthToken;
            }
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
            var tokenPath = Path.Join(Application.persistentDataPath, "deploy_settings.data");
            return tokenPath;
        }

        private static (string, string) GetOwnerAndRepo()
        {
            // limitations: only works on remotes named "origin"
            var (output, _) = RunCommand("git", "remote get-url origin");
            var rx = new Regex("https://github.com/(.*)/(.*).git");
            var matches = rx.Matches(output);
            var owner = matches[0].Groups[1].Value;
            var repo = matches[0].Groups[2].Value;
            return (owner, repo);
        }

        private static string GetGitUser()
        {
            string gitCommand = "config --get user.name";
            var (stdout, _) = RunCommand("git",gitCommand);
            stdout = stdout.Trim();
            return stdout;
        }
        
        private static (string, string) RunCommand(string command, string options)
        {
            // Set up our processInfo to run the command and log to output and errorOutput.
            ProcessStartInfo processInfo = new ProcessStartInfo(command, options) {
                WorkingDirectory = "",
                CreateNoWindow = true,          // We want no visible pop-ups
                UseShellExecute = false,        // Allows us to redirect input, output and error streams
                RedirectStandardOutput = true,  // Allows us to read the output stream
                RedirectStandardError = true    // Allows us to read the error stream
            };

            // Set up the Process
            Process process = new Process {
                StartInfo = processInfo
            };
            process.Start();

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
        
        private static string RunCommandMergeOutputs(string command, string options)
        {
            var (output, errorOutput) = RunCommand(command, options);

            return $"Output:\n{output}\n\nErrorOutput:\n{errorOutput}";  // Return the output
        }
    }
}