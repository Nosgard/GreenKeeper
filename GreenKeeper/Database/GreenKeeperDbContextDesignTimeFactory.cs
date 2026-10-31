using Microsoft.EntityFrameworkCore.Design;

namespace GreenKeeper.Database
{
    /// <summary>
    /// For the EF Core tools (Add-Migration, dotnet ef): GreenKeeperDbContext has no
    /// parameterless constructor, and the app has no DI setup the tools could discover.
    /// </summary>
    public class GreenKeeperDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GreenKeeperDbContext>
    {
        // Delegates to the runtime factory, so both always open the same database file.
        public GreenKeeperDbContext CreateDbContext(string[] args)
        {
            return new GreenKeeperDbContextFactory().CreateDbContext();
        }
    }
}
