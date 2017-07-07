using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.TestHelpers.EF
{
    public static class EFHelpers
    {
        public static T CreateLocalDbContext<T>(string dbName, bool preserveDB = false) where T : DbContext
        {
            var retval = (T)Activator.CreateInstance(typeof(T), GetDbContextOptionsImpl<T>(dbName));
            if (!preserveDB)
            {
                CreateDatabase(dbName, retval);
            }
            return retval;
        }

        private static void CreateDatabase<T>(string dbName, T context = null) where T : DbContext
        {
            if (context == null)
            {
                context = (T)Activator.CreateInstance(typeof(T), GetDbContextOptionsImpl<T>(dbName));
            }
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        public static DbContextOptions<T> GetDbContextOptions<T>(string dbName) where T : DbContext
        {
            var retval = GetDbContextOptionsImpl<T>(dbName);
            CreateDatabase<T>(dbName);
            return retval;
        }
        private static DbContextOptions<T> GetDbContextOptionsImpl<T>(string dbName) where T : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;Connection Timeout=5");
            return optionsBuilder.Options;

        }
    }
}
