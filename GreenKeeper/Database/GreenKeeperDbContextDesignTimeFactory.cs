using Microsoft.EntityFrameworkCore.Design;

namespace GreenKeeper.Database
{
    /// <summary>
    /// For the EF Core tools (Add-Migration, dotnet ef), which cannot create
    /// GreenKeeperDbContext without this class.
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
