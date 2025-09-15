using EcoRide.Server.Model;
using MySql.Data.MySqlClient;
using System.Globalization;

public class CovoiturageService
{
    private readonly string _connectionString;

    public CovoiturageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Covoiturage>> GetAllAsync()
    {
        var covoiturages = new List<Covoiturage>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM covoiturage;", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            covoiturages.Add(new Covoiturage
            {
                Id = Convert.ToInt32(reader["covoiturage id"]),
                DateDepart = Convert.ToDateTime(reader["date_depart"]),
                HeureDepart = TimeOnly.FromDateTime(Convert.ToDateTime(reader["heure_depart"])),
                LieuDepart = reader["lieu_depart"].ToString(),
                DateArrivee = reader["date_arrivee"].ToString(),
                HeureArrivee = reader["heure_arrivee"].ToString(),
                LieuArrivee = reader["lieu_arrivee"].ToString(),
                Statut = reader["statut"] as string,
                NbPlace = Convert.ToInt32(reader["nb_place"]),
                PrixPersonne = Convert.ToDecimal(reader["prix_personne"].ToString(), CultureInfo.InvariantCulture),

            });
        }

        return covoiturages;
    }
}
