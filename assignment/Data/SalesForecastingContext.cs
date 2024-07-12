using Microsoft.EntityFrameworkCore;
using SalesForecastingApp.Data;
using SalesForecastingApp.Models;

namespace SalesForecastingApp.Data
{
    public class SalesForecastingContext : DbContext
    {
        public SalesForecastingContext(DbContextOptions<SalesForecastingContext> options)
            : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Return> Returns { get; set; }


    }
}
