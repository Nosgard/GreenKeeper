using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Database
{
    /// <summary>
    /// Separate factory, used ONLY by the EF Core migration tooling
    /// (Add-Migration / dotnet ef), never by the app itself at runtime.
    /// 
    /// Necessary because GreenKeeperDbContext has no parameterless
    /// constructor (it requires DbContextOptions to be passed in)
    /// and this project has no central DI-Startup configuration
    /// the tooling could otherwise discover automatically
    /// </summary>
    public class GreenKeeperDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GreenKeeperDbContext>
    {
        public GreenKeeperDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GreenKeeperDbContext>();
            optionsBuilder.UseSqlite($"Data Source={DbPathProvider.GetDatabasePath()}");

            return new GreenKeeperDbContext(optionsBuilder.Options);
        }
    }
}
