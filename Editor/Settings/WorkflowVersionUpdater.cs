using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Deploy.Editor.Settings
{
    [InitializeOnLoad]
    public class WorkflowVersionUpdater : IPackageManagerExtension
    {
        static WorkflowVersionUpdater()
        {
            PackageManagerExtensions.RegisterExtension(new WorkflowVersionUpdater());
        }

        public VisualElement CreateExtensionUI()
        {
            return null;
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
            if (UpdatedSelf(packageInfo))
            {
                if (DeploySettings.GetOrCreate().UpdateWorkflowAutomatically)
                {
                    var revision = packageInfo.git.revision;
                    var isMainRev = revision is "main" or "HEAD";
                    var newVersion = isMainRev ? $"v{packageInfo.version}" : revision;
                    SetWorkflowVersion(newVersion);
                }
            }
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }

        private bool UpdatedSelf(PackageInfo packageInfo)
        {
            return packageInfo.name == "com.facticus.deploy";
        }
        
        private static string GetWorkflowPath()
        {
            var deploySettings = DeploySettings.GetOrCreate();
            var filename = deploySettings.WorkflowId;
            var path = Path.Combine(deploySettings.GitDirectory, ".github", "workflows", $"{filename}.yml");
            return path;
        }
        
        private static string GetWorkflowVersion()
        {
            var path = GetWorkflowPath();

            var groupName = "ver";
            var rxPattern = new Regex($"uses: mnicolas94/facticus-deploy/.github/.*/.*@(?<{groupName}>.*)");
            
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var matches = rxPattern.Matches(line);
                foreach (Match match in matches)
                {
                    var ver = match.Groups[groupName];
                    return ver.Value;
                }
            }

            return "NO-VERSION";
        }

        private static void SetWorkflowVersion(string newVersion)
        {
            var path = GetWorkflowPath();

            var groupName = "ver";
            var pattern = $"(uses: mnicolas94/facticus-deploy/.github/.*/.*@)(?<{groupName}>.*)";
            var regex = new Regex(pattern);
            
            var lines = File.ReadAllLines(path);
            var newLines = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var matches = regex.Matches(line);
                foreach (Match match in matches)
                {
                    line = Regex.Replace(line, pattern, $"$1{newVersion}");
                }

                newLines[i] = line;
            }

            var allText = string.Join("\n", newLines);
            File.WriteAllText(path, allText);
        }
    }
}