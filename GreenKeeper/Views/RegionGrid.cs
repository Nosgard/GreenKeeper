using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace GreenKeeper.Views
{
    /// <summary>
    /// A grid with an automation element, so a screen reader user hears its
    /// AutomationProperties.Name on entering it. Without one, the name is never read.
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
