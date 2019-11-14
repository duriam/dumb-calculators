using System.Diagnostics;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Mobge
{
    /// <summary>
    /// Editor extension for starting compilation in the background
    /// </summary>
    public static class UnityCompileInBackground
    {
        // Windows watcher utility constant path
        private const string CONSOLE_APP_PATH = @"Unity-BackgroundCompile-Deamon/Editor/Unity-BackgroundCompile-Deamon.exe";

        // File monitoring tool process
        private static Process _process;
        // Should start compilation
        private static bool _isRefresh;

        /// <summary>
        /// Called when the Unity editor starts.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Init()
        {
            // Start the file monitoring tool
            // Compile starts when a message is received from the tool
            var dataPath = Application.dataPath;
#if UNITY_EDITOR_OSX
            var filename = "/usr/local/bin/fswatch";
#else
            var filename = dataPath + "/" + CONSOLE_APP_PATH;
#endif
            var path = Application.dataPath;
#if UNITY_EDITOR_OSX
            var arguments = string.Format(@"-x ""{0}""", path);
#else
            var arguments = string.Format(@"-p ""{0}"" -w 0", path);
#endif
            var windowStyle = ProcessWindowStyle.Hidden;

            var info = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = windowStyle,
                Arguments = arguments,
            };

            _process = Process.Start(info);
            _process.OutputDataReceived += OnReceived;
            _process.BeginOutputReadLine();

            //UnityEngine.Debug.Log("[UnityCompileInBackground] Start Watching");

            EditorApplication.update += OnUpdate;
            EditorApplication.quitting += OnQuit;
            CompilationPipeline.assemblyCompilationStarted += OnCompilationStarted;
        }

        /// <summary>
        /// Called when the Unity editor exits
        /// </summary>
        private static void OnQuit()
        {
            Dispose();
        }

        /// <summary>
        /// Called when compilation starts
        /// </summary>
        private static void OnCompilationStarted(string _)
        {
            Dispose();
        }

        /// <summary>
        /// Stop file monitoring tool
        /// </summary>
        private static void Dispose()
        {
            // Do nothing if already stopped
            if (_process == null) return;

            if (!_process.HasExited)
                _process.Kill();
            _process.Dispose();
            _process = null;

            //UnityEngine.Debug.Log("[UnityCompileInBackground] Stop Watching");
        }

        /// <summary>
        /// Called when the editor is updated
        /// </summary>
        private static void OnUpdate()
        {
            // If the compile flag is not set, if compiling, or if refreshing, early exit
            if (!_isRefresh) return;
            if (EditorApplication.isCompiling) return;
            if (EditorApplication.isUpdating) return;

            // Start compiling
            //UnityEngine.Debug.Log("[UnityCompileInBackground] Start Compiling");
            _isRefresh = false;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Called when a message is received from the file monitoring tool
        /// </summary>
        private static void OnReceived(object sender, DataReceivedEventArgs e)
        {
            var message = e.Data;

            // Set compile flag if file has been changed or renamed.
            // In this function nothing happens when you call AssetDatabase.Refresh
            // Set only the flag and Refresh is done with EditorApplication.update
#if UNITY_EDITOR_OSX
            if (message.Contains("Created") || message.Contains("Renamed") || message.Contains("Updated") || message.Contains("Removed"))
            {
                _isRefresh = true;
            }
#else
            if (message.Contains("OnChanged") || message.Contains("OnRenamed"))
            {
                _isRefresh = true;
            }
#endif
        }
    }
}

