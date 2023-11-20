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
using UnityEditor;
using UnityEngine;

namespace Deploy.Editor.BackEnds
{
    public static class Utility
    {
        public static void BuildLocally(DeployContext context)
        {
            var buildsDirectory = EditorUtility.OpenFolderPanel("Select builds directory", "", "");

            var elements = context.Platforms.Where(element => element.Enabled).ToList();
            var overrideVariables = context.OverrideVariables.ToList();
            var overridesBackup = overrideVariables.OverrideVariablesToBase64();
            
            for (int i = 0; i < elements.Count; i++)
            {
                var buildDeployElement = elements[i];
                
                // create build folder name
                var buildFolderName = $"{context.name}-{i}-{buildDeployElement.BuildPlatform.GetGameCiName()}";

                // build locally
                var buildPath = BuildLocally(buildDeployElement, buildsDirectory, buildFolderName, overrideVariables);
                buildPath.Replace("\\", "/");
            }
            
            // restore override variables backup
            OverrideVariablesListExtensions.ApplyOverrideVariablesValues(overridesBackup);
        }
        
        public static string BuildLocally(BuildDeployElement element, string buildsDirectory, string buildFolderName,
            List<BuildVariableValue> overrideVariables)
        {
            if (element.BuildPlatform is Dummy)
            {
                Debug.Log("Ignoring Dummy build");
                return "";
            }
            
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
            var buildDir = Path.Combine(buildsDirectory, buildFolderName, buildName);
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
        
        public static string GetAndroidVersionCode(string version)
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
    }
}