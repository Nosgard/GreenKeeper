using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Database
{
    /// <summary>
    /// Runtime factory that creates a fresh, short-lived GreenKeeperDbContext
    /// instance on demand. Used by the repository layer: every single
    /// database operation gets it's own context via this factory,
    /// instead of one context living for the entiere app session - this way,
    /// a failure in one operation (e.g. a locked file, a constraint violation)
    /// can never leave a shared context in a broken state that would affect
    /// subsequent, unrelated operations
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
