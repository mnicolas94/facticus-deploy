using System;
using Deploy.Editor.Settings;
using Deploy.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace Deploy.Editor.Versioning
{
    /// <summary>
    /// Code partially taken from https://github.com/game-ci/unity-builder/blob/main/dist/default-build-script/Assets/Editor/UnityBuilderAction/Versioning/Git.cs
    /// </summary>
    public static class VersionGenerator
    {
        private const string DefaultOnError = "0.0.1";
        private const string GitApp = "git";
        
        public static string GetVersionByStrategy(VersioningStrategy strategy)
        {
            switch (strategy)
            {
                case VersioningStrategy.Semantic:
                    return GenerateSemanticCommitVersion();
                case VersioningStrategy.Tag:
                    return GetTagVersion();
                case VersioningStrategy.None:
                default:
                    return Application.version;
            }
        }

        /// <summary>
        /// Generate a version based on the latest tag and the amount of commits.
        /// Format: 0.1.2 (where 2 is the amount of commits).
        ///
        /// If no tag is present in the repository then v0.0 is assumed.
        /// This would result in 0.0.# where # is the amount of commits.
        /// </summary>
        private static string GenerateSemanticCommitVersion()
        {
            try
            {
                string version;
                if (HasAnyVersionTags()) {
                    version = GetSemanticCommitVersion();
                    Console.WriteLine("Repository has a valid version tag.");
                } else {
                    version = $"0.0.{GetTotalNumberOfCommits()}";
                    Console.WriteLine("Repository does not have tags to base the version on.");
                }

                Console.WriteLine($"Version is {version}");

                return version;
            }
            catch (Exception)
            {
                return DefaultOnError;
            }
        }

        /// <summary>
        /// Get the version of the current tag.
        ///
        /// The tag must point at HEAD for this method to work.
        ///
        /// Output Format:
        /// #.* (where # is the major version and * can be any number of any type of character)
        /// </summary>
        private static string GetTagVersion()
        {
            try
            {
                var args = @"tag --points-at HEAD | grep v[0-9]*";
                var gitDir = DeploySettings.GetOrCreate().GitDirectory;
                var (version, _) = TerminalUtils.RunCommand(GitApp, args, gitDir);
                version = version.Substring(1);
                return version;
            }
            catch (Exception)
            {
                return DefaultOnError;
            }
        }

        /// <summary>
        /// Get the total number of commits.
        /// </summary>
        private static int GetTotalNumberOfCommits()
        {
            var args = @"rev-list --count HEAD";
            var gitDir = DeploySettings.GetOrCreate().GitDirectory;
            var (numberOfCommitsAsString, _) = TerminalUtils.RunCommand(GitApp, args, gitDir);

            return int.Parse(numberOfCommitsAsString);
        }

        /// <summary>
        /// Whether or not the repository has any version tags yet.
        /// </summary>
        private static bool HasAnyVersionTags()
        {
            var args = @"tag --list --merged HEAD | grep v[0-9]* | wc -l";
            var gitDir = DeploySettings.GetOrCreate().GitDirectory;
            var (output, _) = TerminalUtils.RunCommand(GitApp, args, gitDir);
            return "" != output;
        }

        /// <summary>
        /// Retrieves the build version from git based on the most recent matching tag and
        /// commit history. This returns the version as: {major.minor.build} where 'build'
        /// represents the nth commit after the tagged commit.
        /// Note: The initial 'v' and the commit hash are removed.
        /// </summary>
        private static string GetSemanticCommitVersion()
        {
            // v0.1-2-g12345678 (where 2 is the amount of commits, g stands for git)
            string version = GetVersionString();
            // 0.1-2
            version = version.Substring(1, version.LastIndexOf('-') - 1);
            // 0.1.2
            version = version.Replace('-', '.');

            return version;
        }

        /// <summary>
        /// Get version string.
        ///
        /// Format: `v0.1-2-g12345678` (where 2 is the amount of commits since the last tag)
        ///
        /// See: https://softwareengineering.stackexchange.com/questions/141973/how-do-you-achieve-a-numeric-versioning-scheme-with-git
        /// </summary>
        private static string GetVersionString()
        {
            var args = @"describe --tags --long --match ""v[0-9]*""";
            var gitDir = DeploySettings.GetOrCreate().GitDirectory;
            var (version, _) = TerminalUtils.RunCommand(GitApp, args, gitDir);
            return version;
        }
    }
}