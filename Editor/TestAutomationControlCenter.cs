using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.TestAutomation
{
    [InitializeOnLoad]
    internal static class TestAutomationControlCenter
    {
        private const string PackageId = "com.deucarian.test-automation";
        private const string ToolId = "deucarian.test-automation";
        private const string Commands =
            "Run EditMode:\n" +
            "-executeMethod Deucarian.TestAutomation.BatchTestRunner.RunEditMode\n\n" +
            "Run PlayMode:\n" +
            "-executeMethod Deucarian.TestAutomation.BatchTestRunner.RunPlayMode\n\n" +
            "Run both:\n" +
            "-executeMethod Deucarian.TestAutomation.BatchTestRunner.RunAll\n\n" +
            "Add -batchTestResults <path> and optional -batchTestFilter <name>.";

        static TestAutomationControlCenter()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                ToolId,
                "Test Automation",
                "View durable Unity batch-test entry points.",
                DeucarianControlCenterArea.Developer,
                ShowBatchCommands,
                PackageId,
                searchTerms: new[] { "tests", "batch", "editmode", "playmode" },
                order: 300));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private static void ShowBatchCommands()
        {
            EditorUtility.DisplayDialog("Deucarian Test Automation", Commands, "OK");
        }

        private sealed class Provider : IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                yield return new DeucarianControlCenterCard(
                    PackageId + ".developer",
                    DeucarianControlCenterArea.Developer,
                    "Test Automation",
                    "Durable batch-mode EditMode and PlayMode test entry points.",
                    PackageId,
                    DeucarianControlCenterStatus.Success,
                    "Batch runner available",
                    order: 300,
                    details: new[]
                    {
                        "No test names, results, paths, or command-line payloads are captured."
                    },
                    actions: new[]
                    {
                        new DeucarianControlCenterAction(
                            "show-commands",
                            "Show Batch Commands",
                            ShowBatchCommands)
                    },
                    searchTerms: new[] { "tests", "batch", "runner" });
            }
        }
    }
}
