using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.TestHelpers.EF
{
    public static class EFHelpers
    {
        public static T CreateLocalDbContext<T>(string dbName,bool preserveDB= false) where T : DbContext
        {
            var retval = (T)Activator.CreateInstance(typeof(T),GetDbContextOptions<T>(dbName));
            if (!preserveDB)
            {
                retval.Database.EnsureDeleted();
                retval.Database.EnsureCreated();
            }
            return retval;

        }

        private static DbContextOptions<T> GetDbContextOptions<T>(string dbName) where T:DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;Connection Timeout=5");         
            return optionsBuilder.Options;

        }
    }
}
