using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra;

public class Cart
{
    public int CartId { get; set; }

    public string Name { get; set; }
}

public class AppStateDbContext : DbContext
{
    public DbSet<Cart> Carts { get; set; }

    public AppStateDbContext(DbContextOptions<AppStateDbContext> options)
        : base(options)
    {
    }
}
