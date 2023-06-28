using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Deploy.Editor.Utility
{
    public static class TerminalUtils
    {
        public static (string, string) RunCommand(string command, string options, string workingDir,
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
        
        public static string RunCommandMergeOutputs(string command, string options, string workingDir, bool createAsCmdPopup=false)
        {
            var (output, errorOutput) = RunCommand(command, options, workingDir, createAsCmdPopup);

            return $"Output:\n{output}\n\nErrorOutput:\n{errorOutput}";  // Return the output
        }
    }
}