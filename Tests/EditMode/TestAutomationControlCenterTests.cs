using System.Linq;
using Deucarian.Editor;
using NUnit.Framework;

namespace Deucarian.TestAutomation.Tests
{
    public sealed class TestAutomationControlCenterTests
    {
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