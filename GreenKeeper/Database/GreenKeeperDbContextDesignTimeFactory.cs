using Microsoft.EntityFrameworkCore.Design;

namespace GreenKeeper.Database
{
    /// <summary>
    /// Separate factory, used ONLY by the EF Core migration tooling
    /// (Add-Migration / dotnet ef), never by the app itself at runtime.
    /// 
    /// Necessary because GreenKeeperDbContext has no parameterless
    /// constructor (it requires DbContextOptions to be passed in)
    /// and this project has no central DI startup configuration
    /// the tooling could otherwise discover automatically.
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
