using Microsoft.EntityFrameworkCore;

namespace GreenKeeper.Database
{
    /// <summary>
    /// Creates a new GreenKeeperDbContext for every repository operation, so a failed one
    /// cannot leave a shared context broken for the next.
    /// </summary>
    public class GreenKeeperDbContextFactory : IDbContextFactory<GreenKeeperDbContext>
    {
        public GreenKeeperDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<GreenKeeperDbContext>();
            optionsBuilder.UseSqlite($"Data Source={DbPathProvider.GetDatabasePath()}");

            return new GreenKeeperDbContext(optionsBuilder.Options);
        }
    }
}
