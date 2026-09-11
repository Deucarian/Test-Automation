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

        static TestAutomationControlCenter()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                ToolId,
                "Test Automation",
                "View durable Unity batch-test entry points.",
                DeucarianControlCenterArea.Developer,
                ShowBatchCommands,
                PackageId,
                iconKey: DeucarianEditorIconIds.Play, searchTerms: new[] { "tests", "batch", "editmode", "playmode" },
                order: 300, createPage: TestAutomationWorkspace.CreatePage));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private static void ShowBatchCommands()
        {
            DeucarianEditorWindowPages.ShowStandalone<TestAutomationWorkspace>("Test automation",
                new UnityEngine.Vector2(560, 480));
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
                            ShowBatchCommands, navigationToolId: ToolId)
                    },
                    searchTerms: new[] { "tests", "batch", "runner" });
            }
        }
    }
}
