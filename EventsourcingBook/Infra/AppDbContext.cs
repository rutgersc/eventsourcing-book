using EventsourcingBook.Infra.Carts;
using EventsourcingBook.Infra.Inventories;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra;

public class AppDbContext : DbContext
{
    //
    // ReadModels
    //
    public DbSet<InventoriesReadModelEntity> InventoriesReadModel { get; set; }

    public DbSet<CartsWithProductsReadModelEntity> CartsWithProducts { get; set; }

    //
    // State stored deciders
    //
    public DbSet<Cart> Carts { get; set; }

    public DbSet<InventoryEntity> Inventories { get; set; }

    public DbSet<PricingEntity> Pricing { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>()
            .HasKey(c => c.CartId);

        modelBuilder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne() // assuming CartItem has no navigation property back to Cart
            .HasForeignKey(ci => ci.CartId);

        // CartItem
        modelBuilder.Entity<CartItem>()
            .HasKey(ci => ci.CartItemId);

        modelBuilder.Entity<InventoryEntity>()
            .HasKey(e => e.ProductId);

        modelBuilder.Entity<PricingEntity>()
            .HasKey(e => e.ProductId);

        // Reamodels
        modelBuilder.Entity<InventoriesReadModelEntity>()
            .HasNoKey();

        modelBuilder.Entity<CartsWithProductsReadModelEntity>()
            .HasKey(
                nameof(CartsWithProductsReadModelEntity.CartId),
                nameof(CartsWithProductsReadModelEntity.ProductId));
    }
}
