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
        public DbSet<Covoiturage> covoiturage { get; set; }
        public DbSet<Utilisateur> utilisateur { get; set; }
        public DbSet<Voiture> voiture { get; set; }
        public DbSet<Role> role { get; set; }
        public DbSet<Preference> preference { get; set; }
        public DbSet<Possede> possede { get; set; }
        public DbSet<Avis> avis { get; set; }
        public DbSet<Parametre> parametre { get; set; }
        public DbSet<Marque> marque { get; set; }
        public DbSet<Participation> participation { get; set; }
        public DbSet<Depose> depose { get; set; }
        public DbSet<TempAvis> tempAvis { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "server=localhost;database=ecoride;user=root;password=aON8hSy_GeS;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
}

}

