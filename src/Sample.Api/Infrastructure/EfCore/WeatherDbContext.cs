using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;

namespace Sample.Api.Infrastructure.EfCore;

public sealed class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public DbSet<OutBox> OutBox { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutBox>(outBox =>
        {
            outBox.HasKey(x => x.Id);
            outBox.Property(x => x.Type).IsRequired();
            outBox.Property(x => x.Data).IsRequired().HasColumnType("jsonb");
            outBox.Property(x => x.CreatedAtTimestamp).IsRequired();
            outBox.Property(x => x.ProcessedAtTimestamp);
        });
    }
}
