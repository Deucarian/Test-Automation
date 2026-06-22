using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Deucarian.TestAutomation
{
    /// <summary>Reusable Unity batch-mode test entry points for Deucarian package validation projects.</summary>
    public static class BatchTestRunner
    {
        private const string PendingKey = "Deucarian.TestAutomation.Pending";
        private const string ModeKey = "Deucarian.TestAutomation.Mode";
        private const string ResultPathKey = "Deucarian.TestAutomation.ResultPath";
        private const string FilterKey = "Deucarian.TestAutomation.Filter";
        private const string TimeoutKey = "Deucarian.TestAutomation.TimeoutSeconds";
        private const string StartUtcKey = "Deucarian.TestAutomation.StartUtc";
        private const string LogPathKey = "Deucarian.TestAutomation.LogPath";
        private static BatchRunCallbacks _callbacks;
        private static bool _executeRequested;

        public static void RunEditMode() => Start(TestMode.EditMode);
        public static void RunPlayMode() => Start(TestMode.PlayMode);

        public static void RunAll()
        {
            Start(TestMode.EditMode | TestMode.PlayMode);
        }

        private static void Start(TestMode mode)
        {
            BatchRunConfig config = BatchRunConfig.FromCommandLine(mode);
            Persist(config);
            EnsureRegistered();
            Execute(config);
        }

        internal static void EnsureRegistered()
        {
            if (!SessionState.GetBool(PendingKey, false)) return;
            if (_callbacks == null)
            {
                _callbacks = new BatchRunCallbacks(LoadConfig());
                TestRunnerApi.RegisterTestCallback(_callbacks);
            }

            EditorApplication.update -= WatchdogUpdate;
            EditorApplication.update += WatchdogUpdate;
        }

        private static void Execute(BatchRunConfig config)
        {
            if (_executeRequested) return;
            _executeRequested = true;
            var filter = new Filter { testMode = config.Mode };
            if (!string.IsNullOrEmpty(config.Filter))
                filter.testNames = new[] { config.Filter };
            ScriptableObject.CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings(filter));
        }

        private static void WatchdogUpdate()
        {
            if (!SessionState.GetBool(PendingKey, false))
            {
                EditorApplication.update -= WatchdogUpdate;
                return;
            }

            BatchRunConfig config = LoadConfig();
            if (config.TimeoutSeconds <= 0) return;
            double elapsed = (DateTime.UtcNow - config.StartUtc).TotalSeconds;
            if (elapsed < config.TimeoutSeconds) return;
            BatchRunResult timeout = BatchRunResult.Timeout(config, "Timed out after " + config.TimeoutSeconds.ToString(CultureInfo.InvariantCulture) + " seconds.");
            WriteAndExit(timeout);
        }

        private static void Persist(BatchRunConfig config)
        {
            SessionState.SetBool(PendingKey, true);
            SessionState.SetInt(ModeKey, (int)config.Mode);
            SessionState.SetString(ResultPathKey, config.ResultPath);
            SessionState.SetString(FilterKey, config.Filter ?? string.Empty);
            SessionState.SetInt(TimeoutKey, config.TimeoutSeconds);
            SessionState.SetString(StartUtcKey, config.StartUtc.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetString(LogPathKey, config.LogPath ?? string.Empty);
        }

        private static BatchRunConfig LoadConfig()
        {
            DateTime start = DateTime.UtcNow;
            DateTime.TryParse(SessionState.GetString(StartUtcKey, string.Empty), null, DateTimeStyles.RoundtripKind, out start);
            return new BatchRunConfig(
                (TestMode)SessionState.GetInt(ModeKey, (int)TestMode.EditMode),
                SessionState.GetString(ResultPathKey, DefaultResultPath(TestMode.EditMode)),
                SessionState.GetInt(TimeoutKey, 300),
                EmptyToNull(SessionState.GetString(FilterKey, string.Empty)),
                start,
                EmptyToNull(SessionState.GetString(LogPathKey, string.Empty)));
        }

        private static string EmptyToNull(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static string DefaultResultPath(TestMode mode)
        {
            string platform = mode == TestMode.PlayMode ? "playmode" : mode == TestMode.EditMode ? "editmode" : "all";
            return Path.Combine(Environment.CurrentDirectory, "deucarian-test-results-" + platform + ".json").Replace('\\', '/');
        }

        private static void WriteAndExit(BatchRunResult result)
        {
            SessionState.SetBool(PendingKey, false);
            EditorApplication.update -= WatchdogUpdate;
            Directory.CreateDirectory(Path.GetDirectoryName(result.ResultPath));
            File.WriteAllText(result.ResultPath, result.ToJson());
            File.WriteAllText(Path.ChangeExtension(result.ResultPath, ".txt"), result.ToText());
            AssetDatabase.SaveAssets();
            EditorApplication.Exit(result.ExitCode);
        }

        private sealed class BatchRunCallbacks : ICallbacks
        {
            private readonly BatchRunConfig _config;
            private readonly List<BatchFailure> _failures = new List<BatchFailure>();
            private bool _started;

            public BatchRunCallbacks(BatchRunConfig config)
            {
                _config = config;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                _started = true;
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result == null) return;
                if (result.FailCount > 0 || result.TestStatus == TestStatus.Failed)
                    _failures.Add(new BatchFailure(result.FullName, result.ResultState, result.Message, result.StackTrace));
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                BatchRunResult completed = BatchRunResult.Completed(_config, result, _failures, _started);
                WriteAndExit(completed);
            }
        }

        private readonly struct BatchRunConfig
        {
            public BatchRunConfig(TestMode mode, string resultPath, int timeoutSeconds, string filter, DateTime startUtc, string logPath)
            {
                Mode = mode;
                ResultPath = string.IsNullOrWhiteSpace(resultPath) ? DefaultResultPath(mode) : resultPath;
                TimeoutSeconds = timeoutSeconds <= 0 ? 300 : timeoutSeconds;
                Filter = filter;
                StartUtc = startUtc.Kind == DateTimeKind.Utc ? startUtc : startUtc.ToUniversalTime();
                LogPath = logPath;
            }

            public TestMode Mode { get; }
            public string ResultPath { get; }
            public int TimeoutSeconds { get; }
            public string Filter { get; }
            public DateTime StartUtc { get; }
            public string LogPath { get; }

            public static BatchRunConfig FromCommandLine(TestMode mode)
            {
                string[] args = Environment.GetCommandLineArgs();
                string result = null;
                string filter = null;
                string logPath = null;
                int timeout = 300;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-batchTestResults" && i + 1 < args.Length) result = args[++i];
                    else if (args[i] == "-batchTestFilter" && i + 1 < args.Length) filter = args[++i];
                    else if (args[i] == "-batchTestTimeoutSeconds" && i + 1 < args.Length) int.TryParse(args[++i], out timeout);
                    else if (args[i] == "-logFile" && i + 1 < args.Length) logPath = args[++i];
                }
                return new BatchRunConfig(mode, result, timeout, filter, DateTime.UtcNow, logPath);
            }
        }

        private readonly struct BatchFailure
        {
            public BatchFailure(string name, string state, string message, string stackTrace)
            {
                Name = name ?? string.Empty; State = state ?? string.Empty; Message = message ?? string.Empty; StackTrace = stackTrace ?? string.Empty;
            }
            public string Name { get; }
            public string State { get; }
            public string Message { get; }
            public string StackTrace { get; }
        }

        private sealed class BatchRunResult
        {
            public string ResultPath;
            public string UnityVersion;
            public string ProjectPath;
            public string TestPlatform;
            public string StartUtc;
            public string EndUtc;
            public double DurationSeconds;
            public int PassCount;
            public int FailCount;
            public int SkipCount;
            public int InconclusiveCount;
            public string ResultStatus;
            public bool TimedOut;
            public bool CallbackCompleted;
            public int ExitCode;
            public string LogPath;
            public string Message;
            public List<BatchFailure> Failures;

            public static BatchRunResult Completed(BatchRunConfig config, ITestResultAdaptor result, List<BatchFailure> failures, bool started)
            {
                DateTime end = DateTime.UtcNow;
                bool failed = result == null || result.FailCount > 0 || result.TestStatus == TestStatus.Failed;
                return new BatchRunResult
                {
                    ResultPath = config.ResultPath,
                    UnityVersion = Application.unityVersion,
                    ProjectPath = Application.dataPath == null ? string.Empty : Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/'),
                    TestPlatform = config.Mode.ToString(),
                    StartUtc = config.StartUtc.ToString("O", CultureInfo.InvariantCulture),
                    EndUtc = end.ToString("O", CultureInfo.InvariantCulture),
                    DurationSeconds = (end - config.StartUtc).TotalSeconds,
                    PassCount = result == null ? 0 : result.PassCount,
                    FailCount = result == null ? 1 : result.FailCount,
                    SkipCount = result == null ? 0 : result.SkipCount,
                    InconclusiveCount = result == null ? 0 : result.InconclusiveCount,
                    ResultStatus = failed ? "Failed" : "Passed",
                    TimedOut = false,
                    CallbackCompleted = true,
                    ExitCode = failed ? 1 : 0,
                    LogPath = config.LogPath ?? string.Empty,
                    Message = result == null ? "RunFinished supplied no result." : result.Message ?? string.Empty,
                    Failures = failures ?? new List<BatchFailure>()
                };
            }

            public static BatchRunResult Timeout(BatchRunConfig config, string message)
            {
                DateTime end = DateTime.UtcNow;
                return new BatchRunResult
                {
                    ResultPath = config.ResultPath,
                    UnityVersion = Application.unityVersion,
                    ProjectPath = Application.dataPath == null ? string.Empty : Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/'),
                    TestPlatform = config.Mode.ToString(),
                    StartUtc = config.StartUtc.ToString("O", CultureInfo.InvariantCulture),
                    EndUtc = end.ToString("O", CultureInfo.InvariantCulture),
                    DurationSeconds = (end - config.StartUtc).TotalSeconds,
                    ResultStatus = "TimedOut",
                    TimedOut = true,
                    CallbackCompleted = false,
                    ExitCode = 2,
                    LogPath = config.LogPath ?? string.Empty,
                    Message = message,
                    Failures = new List<BatchFailure>()
                };
            }

            public string ToText()
            {
                return "result=" + ResultStatus + Environment.NewLine +
                       "testPlatform=" + TestPlatform + Environment.NewLine +
                       "unityVersion=" + UnityVersion + Environment.NewLine +
                       "projectPath=" + ProjectPath + Environment.NewLine +
                       "passCount=" + PassCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                       "failCount=" + FailCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                       "skipCount=" + SkipCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                       "inconclusiveCount=" + InconclusiveCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                       "durationSeconds=" + DurationSeconds.ToString("F3", CultureInfo.InvariantCulture) + Environment.NewLine +
                       "timedOut=" + TimedOut + Environment.NewLine +
                       "callbackCompleted=" + CallbackCompleted + Environment.NewLine +
                       "exitCode=" + ExitCode.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                       "logPath=" + LogPath + Environment.NewLine +
                       "message=" + Message + Environment.NewLine +
                       "failures=" + Failures.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            }

            public string ToJson()
            {
                var writer = new System.Text.StringBuilder();
                writer.Append("{\n");
                Append(writer, "unityVersion", UnityVersion, true);
                Append(writer, "projectPath", ProjectPath, true);
                Append(writer, "testPlatform", TestPlatform, true);
                Append(writer, "startUtc", StartUtc, true);
                Append(writer, "endUtc", EndUtc, true);
                Append(writer, "durationSeconds", DurationSeconds.ToString("F3", CultureInfo.InvariantCulture), false, true);
                Append(writer, "passCount", PassCount.ToString(CultureInfo.InvariantCulture), false, true);
                Append(writer, "failCount", FailCount.ToString(CultureInfo.InvariantCulture), false, true);
                Append(writer, "skipCount", SkipCount.ToString(CultureInfo.InvariantCulture), false, true);
                Append(writer, "inconclusiveCount", InconclusiveCount.ToString(CultureInfo.InvariantCulture), false, true);
                Append(writer, "resultStatus", ResultStatus, true);
                Append(writer, "logPath", LogPath, true);
                Append(writer, "timedOut", TimedOut ? "true" : "false", false, true);
                Append(writer, "callbackCompleted", CallbackCompleted ? "true" : "false", false, true);
                Append(writer, "processExitRecommendation", ExitCode.ToString(CultureInfo.InvariantCulture), false, true);
                writer.Append("  \"failures\": [");
                for (int i = 0; i < Failures.Count; i++)
                {
                    if (i > 0) writer.Append(",");
                    writer.Append("\n    {");
                    writer.Append("\"name\":\"").Append(Escape(Failures[i].Name)).Append("\",");
                    writer.Append("\"state\":\"").Append(Escape(Failures[i].State)).Append("\",");
                    writer.Append("\"message\":\"").Append(Escape(Failures[i].Message)).Append("\"}");
                }
                if (Failures.Count > 0) writer.Append("\n  ");
                writer.Append("]\n}");
                return writer.ToString();
            }

            private static void Append(System.Text.StringBuilder writer, string name, string value, bool quote, bool comma = true)
            {
                writer.Append("  \"").Append(name).Append("\": ");
                if (quote) writer.Append("\"").Append(Escape(value)).Append("\"");
                else writer.Append(value);
                if (comma) writer.Append(",");
                writer.Append("\n");
            }

            private static string Escape(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }

    [InitializeOnLoad]
    internal static class BatchTestRunnerDomainReloadBridge
    {
        static BatchTestRunnerDomainReloadBridge()
        {
            BatchTestRunner.EnsureRegistered();
        }
    }
}
