using GreenKeeper.Converters;

namespace GreenKeeper.Tests
{
    public class SmokeTest
    {
        // Simple test that confirms the correct setup of the test project
        [Fact]
        public void ProjectSetup_CompilesAndReferencesMainProject()
        {
            // Pure access test.
            // In case the compilation fails, the ProjectReference on GreenKeeper.csproj doesn't work
            var result = TimeUnitConverter.ToDueDateText(null);

            Assert.Equal(string.Empty, result);
        }
    }
}
