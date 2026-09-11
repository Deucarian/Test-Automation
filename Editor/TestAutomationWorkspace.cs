using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.TestAutomation
{
    public sealed class TestAutomationWorkspace : EditorWindow
    {
        private DeucarianEditorPageSession navigation;
        private void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, "test-automation-home", _ => { });
            navigation.Navigate("deucarian.test-automation");
        }
        private void OnDisable() { navigation?.Dispose(); navigation = null; }

        public static IDeucarianEditorPage CreatePage() => CreatePage(value => EditorGUIUtility.systemCopyBuffer = value);

        internal static IDeucarianEditorPage CreatePage(System.Action<string> copyToClipboard)
        {
            if (copyToClipboard == null) throw new System.ArgumentNullException(nameof(copyToClipboard));
            var root = new VisualElement();
            var workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Test automation";
            workspace.Subtitle.text = "Use repeatable test runs.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.test-automation");
            var scroll = Controls.Scroll("test-automation"); workspace.Content.Add(scroll);
            var card = Controls.Panel("test-commands"); scroll.Add(card);
            string[] labels = { "Edit Mode", "Play Mode", "Both" };
            string[] descriptions = { "Command-line arguments for edit mode tests.",
                "Command-line arguments for play mode tests.", "Command-line arguments for running edit mode and play mode tests." };
            string[] methods = { "RunEditMode", "RunPlayMode", "RunAll" };
            int selected = 0;
            var heading = Controls.Label("Selected command · Edit Mode", "dw-section-title");
            var code = new TextField { name = "test-command", multiline = true, isReadOnly = true };
            code.AddToClassList("dw-code");
            var copySelected = Controls.IconButton("Copy Edit Mode command", "file-text", () =>
                copyToClipboard(Arguments(methods[selected])), DeucarianEditorButtonRole.Primary);
            copySelected.name = "test-copy-selected";
            for (int index = 0; index < labels.Length; index++)
            {
                int mode = index;
                var row = Controls.Region("test-mode-" + mode, "dw-action-summary");
                var text = Controls.Region(null, "dw-action-summary-copy");
                text.Add(Controls.Label(labels[index], "dw-row-title"));
                text.Add(Controls.Label(descriptions[index], "dw-muted"));
                row.Add(text);
                var copy = Controls.IconButton("Copy", "file-text", () =>
                {
                    selected = mode;
                    heading.text = "Selected command · " + labels[mode];
                    code.SetValueWithoutNotify(Arguments(methods[mode]));
                    copySelected.Q<Label>().text = "Copy " + labels[mode] + " command";
                    copySelected.tooltip = "Copy " + labels[mode] + " command";
                    copyToClipboard(Arguments(methods[mode]));
                });
                copy.name = "test-copy-" + mode;
                row.Add(copy); card.Add(row);
            }
            card.Add(heading); card.Add(code);
            code.SetValueWithoutNotify(Arguments(methods[0]));
            card.Add(Controls.Actions(copySelected));
            var advanced = new DeucarianEditorWorkspaceForm(scroll).Section("Command options", true);
            advanced.ReadOnly(null, "Result file", () => "-batchTestResults <path>");
            advanced.ReadOnly(null, "Optional filter", () => "-batchTestFilter <name>");
            advanced.Note(() => "Use a separate Unity batch process. Copying a command never starts tests or closes this editor.");
            return new DeucarianEditorPage(root, dispose: workspace.Dispose);
        }

        private static string Arguments(string method) => "-executeMethod Deucarian.TestAutomation.BatchTestRunner." + method;
    }
}
