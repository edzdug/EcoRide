using EcoRide.Server.Model;
using MySqlConnector;
using System.Globalization;
using System.Text.Json;

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

    public async Task<CovoiturageDetailDto?> GetDetailAsync(int id)
    {
        var sql = @"
        SELECT 
            c.covoiturage_id, c.date_depart, c.heure_depart, c.lieu_depart,
            c.date_arrivee, c.heure_arrivee, c.lieu_arrivee, c.statut,
            c.nb_place, c.prix_personne, c.voiture_id,
            v.modele, v.energie, v.preference, v.utilisateur_id, v.marque_id,
            m.libelle as marque_libelle
        FROM covoiturage c
        INNER JOIN voiture v ON c.voiture_id = v.voiture_id
        INNER JOIN marque m ON v.marque_id = m.marque_id
        WHERE c.covoiturage_id = @Id";

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        CovoiturageDetailDto detail = null;

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null; // pas trouvé
            }

            // Mapper covoiturage + voiture + marque
            var covoiturageDto = new CovoiturageDto
            {
                Id = reader.GetInt32("covoiturage_id"),
                DateDepart = reader.GetDateTime("date_depart"),
                HeureDepart = TimeOnly.FromTimeSpan(reader.GetTimeSpan("heure_depart")),
                LieuDepart = reader.GetString("lieu_depart"),
                DateArrivee = reader.GetString("date_arrivee"),
                HeureArrivee = reader.GetString("heure_arrivee"),
                LieuArrivee = reader.GetString("lieu_arrivee"),
                Statut = reader.GetString("statut"),
                NbPlace = reader.GetInt32("nb_place"),
                PrixPersonne = Convert.ToDecimal(reader["prix_personne"].ToString(), CultureInfo.InvariantCulture),
                Voiture_id = reader.GetInt32("voiture_id"),
                Energie = reader.GetString("energie")
            };

            // Parse JSON préférences
            string preferenceJson = reader.IsDBNull(reader.GetOrdinal("preference"))
                ? "{}"
                : reader.GetString("preference");
            Preference preference;
            try
            {
                preference = JsonSerializer.Deserialize<Preference>(preferenceJson);
            }
            catch
            {
                preference = new Preference
                {
                    Fumeur = false,
                    Animal = false,
                    Autre = null
                };
            }

            string marque = reader.GetString("marque_libelle");
            int utilisateurId = reader.GetInt32("utilisateur_id");

            detail = new CovoiturageDetailDto
            {
                Covoiturage = covoiturageDto,
                Marque = marque,
                Modele = reader.GetString("modele"),
                PreferenceConducteur = preference,
                AvisConducteur = new List<AvisDto>()
            };

            // Fermer le reader pour exécuter une nouvelle requête sur la même connexion
            reader.Close();

            // Récupérer les avis du conducteur
            var avisSql = @"
                SELECT a.note, a.commentaire
                FROM avis a
                INNER JOIN depose d ON a.avis_id = d.avis_id
                WHERE d.utilisateur_id = @UtilisateurId";

            using var avisCommand = new MySqlCommand(avisSql, connection);
            avisCommand.Parameters.AddWithValue("@UtilisateurId", utilisateurId);

            using var avisReader = await avisCommand.ExecuteReaderAsync();

            var avisList = new List<AvisDto>();
            while (await avisReader.ReadAsync())
            {
                avisList.Add(new AvisDto
                {
                    Note = avisReader["note"].ToString(),
                    Commentaire = avisReader["commentaire"].ToString()
                });
            }

            detail.AvisConducteur = avisList;
        }

        return detail;
    }




}
