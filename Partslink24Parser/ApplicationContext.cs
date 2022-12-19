using Microsoft.EntityFrameworkCore;
using Partslink24Parser.Entities;

namespace Partslink24Parser
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

        }

        public static ApplicationContext GetSqlLiteContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>();

            options.UseSqlite($"DataSource=file:C:\\Users\\lifebookE\\source\\repos\\Partslink24Parser\\Partslink24Parser\\DB\\data_vin.db?&cache=shared");

            var context = new ApplicationContext(options.Options);

            return context;
        }

        public DbSet<VehicleData> VehicleData { get; set; }

        public DbSet<MajorCategory> MajorСategories { get; set; }

        public DbSet<MinorCategory> MinorCategories { get; set; }

        public DbSet<Part> Parts { get; set; }

        public DbSet<Point> Points { get; set; }

        public DbSet<PartInformation> PartInformation { get; set; }

    }
}