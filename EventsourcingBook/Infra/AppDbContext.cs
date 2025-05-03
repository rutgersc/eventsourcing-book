using EventsourcingBook.Infra.Carts;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra;

public class AppDbContext : DbContext
{
    public DbSet<Cart> Carts { get; set; }

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
    }
}
