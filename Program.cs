using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace EFCoreIssue12834
{
    public static class Program
    {
        static readonly LoggerFactory ConsoleLoggerFactory =
            new LoggerFactory(new[] { new ConsoleLoggerProvider((_, level) => level >= LogLevel.Information, true) });

        public static void Main(string[] args)
        {
            var options =
                new DbContextOptionsBuilder<SomeDbContext>()
                    .UseNpgsql(args[0])
                    .UseLoggerFactory(ConsoleLoggerFactory)
                    .Options;

            using (var ctx = new SomeDbContext(options))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();

                var a =
                    new SomeEntity
                    {
                        Id = 1,
                        SomeEnum = SomeEnum.Happy,
                        OtherEntity = new OtherEntity { Id = 1 }
                    };

                var b =
                    new SomeEntity
                    {
                        Id = 2,
                        SomeEnum = SomeEnum.Sad,
                        OtherEntity = new OtherEntity { Id = 2 }
                    };

                ctx.SomeEntities.Add(a);
                ctx.SomeEntities.Add(b);
                ctx.SaveChanges();

                a.OtherEntity.SomeEntityId = a.Id;
                b.OtherEntity.SomeEntityId = b.Id;
                ctx.SaveChanges();

                var result =
                    ctx.OtherEntities
                       .Select(e => e.SomeEntity.SomeEnum)
                       .ToArray();

                Console.WriteLine(result.Length);
            }
        }
    }

    public class SomeDbContext : DbContext
    {
        public DbSet<SomeEntity> SomeEntities { get; set; }

        public DbSet<OtherEntity> OtherEntities { get; set; }

        public SomeDbContext(DbContextOptions<SomeDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<SomeEntity>().Property(x => x.SomeEnum).HasConversion<string>();
    }

    public class SomeEntity
    {
        public long Id { get; set; }
        public SomeEnum SomeEnum { get; set; }
        public long SomeOtherEntityId { get; set; }
        public OtherEntity OtherEntity { get; set; }
    }

    public class OtherEntity
    {
        public long Id { get; set; }
        public long? SomeEntityId { get; set; }
        public SomeEntity SomeEntity { get; set; }
    }

    public enum SomeEnum
    {
        Happy,
        Sad
    }
}