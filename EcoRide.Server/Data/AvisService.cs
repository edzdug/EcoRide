using EcoRide.Server.Model;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Globalization;
using BCrypt.Net;

public class AvisService
{
    private readonly string _connectionString;
    private readonly CovoiturageService _covoiturageService;
    private readonly UtilisateurService _utilisateurService;

    public AvisService(IConfiguration configuration, CovoiturageService covoiturageService, UtilisateurService utilisateurService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _covoiturageService = covoiturageService;
        _utilisateurService = utilisateurService;
    }

    public async Task EnvoieAvisAsync(AvisDto dto, int covoiturageId, int utilisateurId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        INSERT INTO temp_avis (commentaire, note, statut, utilisateur_id, covoiturage_id)
        VALUES (@commentaire, @note, @statut, @utilisateurId, @covoiturageId);", connection);

        command.Parameters.AddWithValue("@commentaire", dto.Commentaire ?? "");
        command.Parameters.AddWithValue("@note", dto.Note ?? "");
        command.Parameters.AddWithValue("@statut", "non_valider");
        command.Parameters.AddWithValue("@utilisateurId", utilisateurId);
        command.Parameters.AddWithValue("@covoiturageId", covoiturageId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<TempAvis>> GetAvisEnAttenteAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        SELECT temp_avis_id, commentaire, note, statut, utilisateur_id, covoiturage_id
        FROM temp_avis
        WHERE statut = 'non_valider';", connection);

        var reader = await command.ExecuteReaderAsync();

        var result = new List<TempAvis>();

        while (await reader.ReadAsync())
        {
            var avis = new Avis
            {
                Id = reader.GetInt32("temp_avis_id"),
                Commentaire = reader.GetString("commentaire"),
                // Note est un string dans la base (note varchar), donc on utilise GetString
                Note = int.TryParse(reader.GetString("note"), out var parsedNote) ? parsedNote : 0,
                Statut = reader.GetString("statut")
            };

            var tempAvis = new TempAvis
            {
                avis = avis,
                utilisateur_id = reader.GetInt32("utilisateur_id"),
                covoiturage_id = reader.GetInt32("covoiturage_id")
            };

            result.Add(tempAvis);
        }

        return result;
    }

    public async Task<List<ProblemeAvis>> GetAvisRefuseAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
    SELECT 
        ta.temp_avis_id,
        ta.commentaire,
        ta.note,
        ta.statut,

        passager.utilisateur_id AS passager_id,
        passager.nom AS passager_nom,
        passager.prenom AS passager_prenom,
        passager.email AS passager_email,
        passager.telephone AS passager_telephone,
        passager.adresse AS passager_adresse,
        passager.date_naissance AS passager_date_naissance,
        passager.pseudo AS passager_pseudo,

        chauffeur.utilisateur_id AS chauffeur_id,
        chauffeur.nom AS chauffeur_nom,
        chauffeur.prenom AS chauffeur_prenom,
        chauffeur.email AS chauffeur_email,
        chauffeur.telephone AS chauffeur_telephone,
        chauffeur.adresse AS chauffeur_adresse,
        chauffeur.date_naissance AS chauffeur_date_naissance,
        chauffeur.pseudo AS chauffeur_pseudo,

        c.covoiturage_id,
        c.date_depart,
        c.heure_depart,
        c.lieu_depart,
        c.date_arrivee,
        c.heure_arrivee,
        c.lieu_arrivee,
        c.statut AS covoiturage_statut,
        c.nb_place,
        c.prix_personne

    FROM temp_avis ta
    JOIN utilisateur passager ON ta.utilisateur_id = passager.utilisateur_id
    JOIN covoiturage c ON ta.covoiturage_id = c.covoiturage_id
    JOIN voiture v ON c.voiture_id = v.voiture_id
    JOIN utilisateur chauffeur ON v.utilisateur_id = chauffeur.utilisateur_id

    WHERE ta.statut = 'refuse';", connection);

        var reader = await command.ExecuteReaderAsync();
        var result = new List<ProblemeAvis>();

        while (await reader.ReadAsync())
        {
            var pb = new ProblemeAvis
            {
                avis = new Avis
                {
                    Id = reader.GetInt32("temp_avis_id"),
                    Commentaire = reader.GetString("commentaire"),
                    Note = int.TryParse(reader.GetString("note"), out var parsedNote) ? parsedNote : 0,
                    Statut = reader.GetString("statut")
                },
                passager = new UtilisateurDto
                {
                    Id = reader.GetInt32("passager_id"),
                    Nom = reader.GetString("passager_nom"),
                    Prenom = reader.GetString("passager_prenom"),
                    Email = reader.GetString("passager_email"),
                    Telephone = reader.GetString("passager_telephone"),
                    Adresse = reader.GetString("passager_adresse"),
                    DateNaissance = reader.GetString("passager_date_naissance"),
                    Pseudo = reader.GetString("passager_pseudo"),
                    Photo = null 
                },
                chauffeur = new UtilisateurDto
                {
                    Id = reader.GetInt32("chauffeur_id"),
                    Nom = reader.GetString("chauffeur_nom"),
                    Prenom = reader.GetString("chauffeur_prenom"),
                    Email = reader.GetString("chauffeur_email"),
                    Telephone = reader.GetString("chauffeur_telephone"),
                    Adresse = reader.GetString("chauffeur_adresse"),
                    DateNaissance = reader.GetString("chauffeur_date_naissance"),
                    Pseudo = reader.GetString("chauffeur_pseudo"),
                    Photo = null
                },
                covoiturage = new Covoiturage
                {
                    Id = reader.GetInt32("covoiturage_id"),
                    DateDepart = reader.GetDateTime("date_depart"),
                    HeureDepart = TimeOnly.FromTimeSpan(reader.GetTimeSpan("heure_depart")),
                    LieuDepart = reader.GetString("lieu_depart"),
                    DateArrivee = reader.GetString("date_arrivee"),
                    HeureArrivee = reader.GetString("heure_arrivee"),
                    LieuArrivee = reader.GetString("lieu_arrivee"),
                    Statut = reader.GetString("covoiturage_statut"),
                    NbPlace = reader.GetInt32("nb_place"),
                    PrixPersonne = Convert.ToDecimal(reader["prix_personne"].ToString(), CultureInfo.InvariantCulture),
                }
            };

            result.Add(pb);
        }

        return result;
    }



    public async Task VerifAvisAsync(TempAvis item, string etat)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            if(etat == "valide") { 
                // 1. Insérer dans la table 'avis'
            var insertAvisCommand = new MySqlCommand(@"
            INSERT INTO avis (commentaire, note, statut)
            VALUES (@commentaire, @note, @statut);
            SELECT LAST_INSERT_ID();", connection, (MySqlTransaction)transaction);

            insertAvisCommand.Parameters.AddWithValue("@commentaire", item.avis.Commentaire ?? "");
            insertAvisCommand.Parameters.AddWithValue("@note", item.avis.Note);
            insertAvisCommand.Parameters.AddWithValue("@statut", etat);

            var avisId = Convert.ToInt32(await insertAvisCommand.ExecuteScalarAsync());

                // 2. Récupérer  l'Id du chauffeur
            var getChauffeurCommand = new MySqlCommand(@"
            SELECT v.utilisateur_id
            FROM covoiturage c
            JOIN voiture v ON c.voiture_id = v.voiture_id
            WHERE c.covoiturage_id = @covoiturageId;", connection, (MySqlTransaction)transaction);

            getChauffeurCommand.Parameters.AddWithValue("@covoiturageId", item.covoiturage_id);

            var chauffeurIdObj = await getChauffeurCommand.ExecuteScalarAsync();

            int chauffeurId = Convert.ToInt32(chauffeurIdObj);

                // 3. Insérer dans la table 'depose'
                var insertDeposeCommand = new MySqlCommand(@"
            INSERT INTO depose (utilisateur_id, avis_id)
            VALUES (@chauffeurId, @avisId);", connection, (MySqlTransaction)transaction);

            insertDeposeCommand.Parameters.AddWithValue("@chauffeurId", chauffeurId);
            insertDeposeCommand.Parameters.AddWithValue("@avisId", avisId);

            await insertDeposeCommand.ExecuteNonQueryAsync();


            // 4. Supprimer l'entrée dans la table 'temp_avis'
            var deleteTempCommand = new MySqlCommand(@"
            DELETE FROM temp_avis
            WHERE utilisateur_id = @utilisateurId
              AND covoiturage_id = @covoiturageId;", connection, (MySqlTransaction)transaction);

            deleteTempCommand.Parameters.AddWithValue("@utilisateurId", item.utilisateur_id);
            deleteTempCommand.Parameters.AddWithValue("@covoiturageId", item.covoiturage_id);

            await deleteTempCommand.ExecuteNonQueryAsync();
            }

            if (etat == "refuse")
            {
                var refuseCommand = new MySqlCommand(@"
                UPDATE temp_avis
                SET statut = 'refuse'
                WHERE utilisateur_id = @utilisateurId
                  AND covoiturage_id = @covoiturageId;", connection, (MySqlTransaction)transaction);

                refuseCommand.Parameters.AddWithValue("@utilisateurId", item.utilisateur_id);
                refuseCommand.Parameters.AddWithValue("@covoiturageId", item.covoiturage_id);

                await refuseCommand.ExecuteNonQueryAsync();
            }

                // 5. Commit transaction
                await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("Erreur lors de l'acceptation de l'avis : " + ex.Message);
            throw; // ou retourner une réponse d'erreur API si dans un contrôleur
        }
    }

    public async Task ValidePbAvisAsync(ProblemeAvis item)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            
                // 1. Insérer dans la table 'avis'
                var insertAvisCommand = new MySqlCommand(@"
            INSERT INTO avis (commentaire, note, statut)
            VALUES (@commentaire, @note, @statut);
            SELECT LAST_INSERT_ID();", connection, (MySqlTransaction)transaction);

                insertAvisCommand.Parameters.AddWithValue("@commentaire", item.avis.Commentaire ?? "");
                insertAvisCommand.Parameters.AddWithValue("@note", item.avis.Note);
                insertAvisCommand.Parameters.AddWithValue("@statut", "valide");

                var avisId = Convert.ToInt32(await insertAvisCommand.ExecuteScalarAsync());

                // 2. Insérer dans la table 'depose'
                var insertDeposeCommand = new MySqlCommand(@"
            INSERT INTO depose (utilisateur_id, avis_id)
            VALUES (@chauffeurId, @avisId);", connection, (MySqlTransaction)transaction);

                insertDeposeCommand.Parameters.AddWithValue("@chauffeurId", item.chauffeur.Id);
                insertDeposeCommand.Parameters.AddWithValue("@avisId", avisId);

                await insertDeposeCommand.ExecuteNonQueryAsync();


                // 3. Supprimer l'entrée dans la table 'temp_avis'
                var deleteTempCommand = new MySqlCommand(@"
            DELETE FROM temp_avis
            WHERE utilisateur_id = @utilisateurId
              AND covoiturage_id = @covoiturageId;", connection, (MySqlTransaction)transaction);

                deleteTempCommand.Parameters.AddWithValue("@utilisateurId", item.passager.Id);
                deleteTempCommand.Parameters.AddWithValue("@covoiturageId", item.covoiturage.Id);

                await deleteTempCommand.ExecuteNonQueryAsync();

            // 4. Commit transaction
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("Erreur lors de l'acceptation de l'avis : " + ex.Message);
            throw; // ou retourner une réponse d'erreur API si dans un contrôleur
        }
    }
}