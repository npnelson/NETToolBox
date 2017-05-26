using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetToolBox.TestHelpers.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetToolBox.Tests
{
    public class EFHelperTests
    {
        [Fact]
        public async Task CreateContextTest()
        {
            var context = EFHelpers.CreateLocalDbContext<EFContext>("EFHelperTests");
            context.EFRecords.Add(new EFRecord { EFValue = "TEST" });
            await context.SaveChangesAsync();

            var newContext= EFHelpers.CreateLocalDbContext<EFContext>("EFHelperTests",true);
            var recCount = await newContext.EFRecords.CountAsync();

            recCount.Should().Be(1);
        }
    }

    public class EFRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EFRecordID { get; set; }
        public string EFValue { get; set; }
    }

    public class EFContext : DbContext
    {
        public EFContext(DbContextOptions<EFContext> options):base(options)
        {

        }
        public DbSet<EFRecord> EFRecords { get; set; }
    }
}
