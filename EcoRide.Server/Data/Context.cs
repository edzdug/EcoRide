using EcoRide.Server.Model;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;

namespace EcoRide.Server.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Exemple de table
        public DbSet<covoiturage> covoiturage { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "server=localhost;database=ecoride;user=root;password=aON8hSy_GeS;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
}

}

