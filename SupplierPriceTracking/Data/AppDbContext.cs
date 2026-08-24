using Microsoft.EntityFrameworkCore;
using SupplierPriceTracking.Models;

namespace SupplierPriceTracking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<PriceQuote> PriceQuotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PriceQuote - Price için hassasiyet ayarı (18 basamak, 2 ondalık)
            modelBuilder.Entity<PriceQuote>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}