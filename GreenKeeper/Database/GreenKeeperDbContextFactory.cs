using Microsoft.EntityFrameworkCore;

namespace GreenKeeper.Database
{
    /// <summary>
    /// Creates a new GreenKeeperDbContext for every database operation of the repository.
    /// A failed operation (a locked file, a violated constraint) then cannot leave a shared
    /// context broken for the next one.
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
