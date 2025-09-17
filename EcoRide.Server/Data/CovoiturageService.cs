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

    public async Task<List<CovoiturageDto>> GetAllAsync()
    {
        var covoiturages = new List<CovoiturageDto>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT c.`covoiturage_id`, c.`date_depart`, " +
            "c.`heure_depart`, c.`lieu_depart`,c.`date_arrivee`, " +
            "c.`heure_arrivee`,c.`lieu_arrivee`,c.`statut`," +
            "c.`nb_place`,c.`prix_personne`,c.`voiture_id`," +
            "v.`energie` , MIN(CAST(a.note AS DECIMAL)) AS note_minimale " +
            "FROM covoiturage c JOIN voiture v ON c.voiture_id = v.voiture_id JOIN utilisateur u ON v.utilisateur_id = u.utilisateur_id " +
            "LEFT JOIN depose d ON d.utilisateur_id = u.utilisateur_id LEFT JOIN avis a ON d.avis_id = a.avis_id " +
            "GROUP BY c.covoiturage_id", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            covoiturages.Add(new CovoiturageDto
            {
                Id = Convert.ToInt32(reader["covoiturage_id"]),
                DateDepart = Convert.ToDateTime(reader["date_depart"]),
                HeureDepart = TimeOnly.FromTimeSpan((TimeSpan)reader["heure_depart"]),
                LieuDepart = reader["lieu_depart"].ToString(),
                DateArrivee = reader["date_arrivee"].ToString(),
                HeureArrivee = reader["heure_arrivee"].ToString(),
                LieuArrivee = reader["lieu_arrivee"].ToString(),
                Statut = reader["statut"] as string,
                NbPlace = Convert.ToInt32(reader["nb_place"]),
                PrixPersonne = Convert.ToDecimal(reader["prix_personne"].ToString(), CultureInfo.InvariantCulture),
                Voiture_id = Convert.ToInt32(reader["voiture_id"]),
                Energie = reader["energie"].ToString(),
                NoteMinimale = reader["note_minimale"] != DBNull.Value ? Convert.ToDecimal(reader["note_minimale"]) : null
            });
        }

        return covoiturages;
    }
}
