using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;

namespace Sample.Api.Infrastructure.EfCore;

public sealed class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<OutBox> OutBox { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(x => x.Id);
            product.Property(x => x.Name).IsRequired();
            product.Property(x => x.Description).IsRequired();
            product.Property(x => x.Price).IsRequired();
            product.Property(x => x.Quantity).IsRequired();
        });
        modelBuilder.Entity<Order>(order =>
        {
            order.HasKey(x => x.Id);
            order.OwnsMany(x => x.Items, item =>
            {
                item.ToJson();
            });
        });
        modelBuilder.Entity<OutBox>(outBox =>
        {
            outBox.HasKey(x => x.Id);
            outBox.Property(x => x.Type).IsRequired();
            outBox.Property(x => x.Data).IsRequired().HasColumnType("jsonb");
            outBox.Property(x => x.CreatedAtTimestamp).IsRequired();
            outBox.Property(x => x.ProcessedAtTimestamp);
        });
    }

    public void CleanAllTables()
    {
        Products.ExecuteDelete();
        OutBox.ExecuteDelete();
        Orders.ExecuteDelete();
    }
}
