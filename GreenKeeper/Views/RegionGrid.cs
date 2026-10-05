using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace GreenKeeper.Views
{
    /// <summary>
    /// A grid a screen reader knows as a region of the window, by its
    /// AutomationProperties.Name: on entering it, the user hears where they are. A
    /// plain grid has no automation element, so a name set on it is never read.
    /// </summary>
    public class RegionGrid : Grid
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RegionGridAutomationPeer(this);
        }

        private sealed class RegionGridAutomationPeer(RegionGrid owner) : FrameworkElementAutomationPeer(owner)
        {
            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.Group;
            }

            protected override string GetClassNameCore()
            {
                return nameof(RegionGrid);
            }
        }
    }
}
