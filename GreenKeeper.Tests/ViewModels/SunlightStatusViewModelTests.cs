using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Passive;

namespace GreenKeeper.Tests.ViewModels
{
    public class SunlightStatusViewModelTests
    {
        [Theory]
        [InlineData(SunlightPeriod.Day, "6h / day")]
        [InlineData(SunlightPeriod.Week, "6h / week")]
        [InlineData(SunlightPeriod.Month, "6h / month")]
        [InlineData(SunlightPeriod.Year, "6h / year")]
        public void StatusText_GivenPeriod_ShowsTheHoursPerThatPeriod(SunlightPeriod period, string expected)
        {
            // Given: a sunlight card for 6 hours in the given period
            var requirement = new SunlightRequirement { Hours = 6, Period = period };
            var card = new SunlightStatusViewModel(requirement, onEdit: () => { }, onRemove: () => Task.CompletedTask);

            // When: the status text is read
            var statusText = card.StatusText;

            // Then: it names the hours and the period they refer to
            Assert.Equal(expected, statusText);
        }
    }
}
