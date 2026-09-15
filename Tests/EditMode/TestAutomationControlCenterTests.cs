using System.Linq;
using Deucarian.Editor;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.TestAutomation.Tests
{
    public sealed class TestAutomationControlCenterTests
    {
        private sealed class TestWindow : EditorWindow { }

        [UnityTest]
        public IEnumerator NativeCommandsCopyOnlyTheChosenEntryPointWithoutRunningTests()
        {
            string clipboard = null;
            var window = ScriptableObject.CreateInstance<TestWindow>(); window.Show();
            window.position = new Rect(40, 40, 1586, 940);
            try
            {
                using (var page = TestAutomationWorkspace.CreatePage(value => clipboard = value))
                {
                    window.rootVisualElement.Add(page.Root);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    Assert.That(page.Root.Query<IMGUIContainer>().ToList(), Is.Empty);
                    string[] methods = { "RunEditMode", "RunPlayMode", "RunAll" };
                    for (int index = 0; index < methods.Length; index++)
                    {
                        var button = page.Root.Q<Button>("test-copy-" + index);
                        button.Focus();
                        yield return null;
                        using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = button; button.SendEvent(submit); }
                        string expected = "-executeMethod Deucarian.TestAutomation.BatchTestRunner." + methods[index];
                        Assert.That(clipboard, Is.EqualTo(expected));
                        Assert.That(page.Root.Q<TextField>("test-command").value, Is.EqualTo(expected));
                    }
                    Assert.That(EditorApplication.isPlayingOrWillChangePlaymode, Is.False);
                    Assert.That(SessionState.GetBool("Deucarian.TestAutomation.Pending", false), Is.False);
                }
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void ContributionRegistersStableDeveloperToolAndCommandsAction()
        {
            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture();
            DeucarianToolDescriptor tool = snapshot.Tools.Single(candidate =>
                candidate.Id == "deucarian.test-automation");
            DeucarianControlCenterCard card = snapshot.Cards.Single(candidate =>
                candidate.Id == "com.deucarian.test-automation.developer");

            Assert.That(tool.Area, Is.EqualTo(DeucarianControlCenterArea.Developer));
            Assert.That(card.Area, Is.EqualTo(DeucarianControlCenterArea.Developer));
            CollectionAssert.AreEqual(
                new[] { "show-commands" },
                card.Actions.Select(action => action.Id).ToArray());
        }
    }
}
