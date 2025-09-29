using EcoRide.Server.Model;
using MySqlConnector;
using System.Globalization;
using System.Text.Json;

public class CovoiturageService
{
    private readonly string _connectionString;
    private readonly UtilisateurService _utilisateurService;

    public CovoiturageService(IConfiguration configuration, UtilisateurService utilisateurService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _utilisateurService = utilisateurService;
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

    public async Task<Covoiturage?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM covoiturage WHERE covoiturage_id = @id;", connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Covoiturage
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
                Voiture_id = Convert.ToInt32(reader["voiture_id"])
            };
        }

        return null;
    }

    public async Task<List<Covoiturage>> GetByVoitureIdAsync(int voitureId)
    {
        var covoiturages = new List<Covoiturage>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        SELECT * FROM covoiturage 
        WHERE voiture_id = @voitureId;", connection);

        command.Parameters.AddWithValue("@voitureId", voitureId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            covoiturages.Add(new Covoiturage
            {
                Id = Convert.ToInt32(reader["covoiturage_id"]),
                DateDepart = Convert.ToDateTime(reader["date_depart"]),
                HeureDepart = TimeOnly.FromTimeSpan((TimeSpan)reader["heure_depart"]),
                LieuDepart = reader["lieu_depart"].ToString(),
                DateArrivee = reader["date_arrivee"].ToString(),
                HeureArrivee = reader["heure_arrivee"].ToString(),
                LieuArrivee = reader["lieu_arrivee"].ToString(),
                Statut = reader["statut"].ToString(),
                NbPlace = Convert.ToInt32(reader["nb_place"]),
                PrixPersonne = Convert.ToDecimal(reader["prix_personne"].ToString(), CultureInfo.InvariantCulture),
                Voiture_id = Convert.ToInt32(reader["voiture_id"])
            });
        }

        return covoiturages;
    }

    public async Task DecrementerPlaceAsync(int covoiturageId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        UPDATE covoiturage
        SET nb_place = nb_place - 1
        WHERE covoiturage_id = @id AND nb_place > 0;", connection);

        command.Parameters.AddWithValue("@id", covoiturageId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AjouterCovoiturageAsync(AjouterCovoiturageRequest request)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
            INSERT INTO covoiturage (
                date_depart, heure_depart, lieu_depart,
                date_arrivee, heure_arrivee, lieu_arrivee,
                statut, nb_place, prix_personne, voiture_id
            )
            VALUES (
                @dateDepart, @heureDepart, @lieuDepart,
                @dateArrivee, @heureArrivee, @lieuArrivee,
                @statut, @nbPlace, @prix, @voitureId);", connection);

        command.Parameters.AddWithValue("@dateDepart", request.DateDepart.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@heureDepart", request.HeureDepart.ToString(@"hh\:mm\:ss"));
        command.Parameters.AddWithValue("@lieuDepart", request.LieuDepart);

        command.Parameters.AddWithValue("@dateArrivee", request.DateArrivee);
        command.Parameters.AddWithValue("@heureArrivee", request.HeureArrivee);
        command.Parameters.AddWithValue("@lieuArrivee", request.LieuArrivee);

        command.Parameters.AddWithValue("@statut", "ouvert");
        command.Parameters.AddWithValue("@nbPlace", request.NbPlace);
        command.Parameters.AddWithValue("@prix", request.PrixPersonne.ToString("F2", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@voitureId", request.VoitureId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AnnulerCovoiturageParChauffeurAsync(int covoiturageId, int chauffeurId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. Récupérer les participants du covoiturage
            var getParticipantsCommand = new MySqlCommand(@"
            SELECT utilisateur_id 
            FROM participation 
            WHERE covoiturage_id = @covoiturageId;",
                connection, (MySqlTransaction)transaction);

            getParticipantsCommand.Parameters.AddWithValue("@covoiturageId", covoiturageId);

            var participantIds = new List<int>();
            using var reader = await getParticipantsCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                participantIds.Add(reader.GetInt32(0));
            }
            await reader.CloseAsync(); // important

            // 2. Supprimer les participations (tous)
            var deleteParticipationCommand = new MySqlCommand(@"
            DELETE FROM participation 
            WHERE covoiturage_id = @covoiturageId;",
                connection, (MySqlTransaction)transaction);

            deleteParticipationCommand.Parameters.AddWithValue("@covoiturageId", covoiturageId);
            await deleteParticipationCommand.ExecuteNonQueryAsync();

            // 3. Mettre à jour le statut du covoiturage
            var updateStatusCommand = new MySqlCommand(@"
            UPDATE covoiturage 
            SET statut = 'annuler' 
            WHERE covoiturage_id = @covoiturageId;",
                connection, (MySqlTransaction)transaction);

            updateStatusCommand.Parameters.AddWithValue("@covoiturageId", covoiturageId);
            await updateStatusCommand.ExecuteNonQueryAsync();

            // 4. Remboursements
            foreach (var utilisateurId in participantIds)
            {
                if (utilisateurId == chauffeurId)
                {
                    // Le chauffeur récupère 2 crédits
                    var remboursementChauffeur = new MySqlCommand(@"
                    UPDATE utilisateur 
                    SET credit = credit + 2 
                    WHERE utilisateur_id = @utilisateurId;",
                        connection, (MySqlTransaction)transaction);

                    remboursementChauffeur.Parameters.AddWithValue("@utilisateurId", utilisateurId);
                    await remboursementChauffeur.ExecuteNonQueryAsync();
                }
                else
                {
                    // Les passagers sont remboursés dynamiquement
                    await _utilisateurService.RembourserCreditAsync(utilisateurId, covoiturageId);
                }
            }

            // 5. Commit final
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}
