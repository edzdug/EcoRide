using EcoRide.Server.Model;
using MySqlConnector;
using System.Globalization;
using System.Text.Json;

namespace EcoRide.Server.Data
{
    public class ParticipationService
    {
        private readonly string _connectionString;
        private readonly UtilisateurService _utilisateurService;

        public ParticipationService(IConfiguration configuration, UtilisateurService utilisateurService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _utilisateurService = utilisateurService;
        }
        public async Task AjouterParticipationAsync(int utilisateurId, int covoiturageId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(@"
        INSERT INTO participation (utilisateur_id, covoiturage_id)
        VALUES (@utilisateurId, @covoiturageId);", connection);

            command.Parameters.AddWithValue("@utilisateurId", utilisateurId);
            command.Parameters.AddWithValue("@covoiturageId", covoiturageId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int[]> RechercheParticipationAsync(int utilisateurId)
        {
            var participations = new List<int>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(@"
        SELECT covoiturage_id FROM participation 
        WHERE utilisateur_id = @utilisateurId;", connection);

            command.Parameters.AddWithValue("@utilisateurId", utilisateurId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                participations.Add(reader.GetInt32(0)); // 0 = première colonne : covoiturage_id
            }

            return participations.ToArray();
        }


        public async Task<bool> ExisteParticipationAsync(int utilisateurId, int covoiturageId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(@"
        SELECT COUNT(*) FROM participation
        WHERE utilisateur_id = @utilisateurId AND covoiturage_id = @covoiturageId;", connection);

            command.Parameters.AddWithValue("@utilisateurId", utilisateurId);
            command.Parameters.AddWithValue("@covoiturageId", covoiturageId);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task AnnulerParticipationAsync(int covoiturageId, int utilisateurId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Supprimer la participation
                var deleteCommand = new MySqlCommand(@"
            DELETE FROM participation 
            WHERE covoiturage_id = @covoiturageId AND utilisateur_id = @utilisateurId;",
                    connection, (MySqlTransaction)transaction);

                deleteCommand.Parameters.AddWithValue("@covoiturageId", covoiturageId);
                deleteCommand.Parameters.AddWithValue("@utilisateurId", utilisateurId);

                var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new Exception("Aucune participation trouvée à annuler.");

                // 2. Incrémenter le nombre de places disponibles du covoiturage
                var majPlacesCommand = new MySqlCommand(@"
            UPDATE covoiturage 
            SET nb_place = nb_place + 1 
            WHERE covoiturage_id = @covoiturageId;",
                    connection, (MySqlTransaction)transaction);

                majPlacesCommand.Parameters.AddWithValue("@covoiturageId", covoiturageId);
                await majPlacesCommand.ExecuteNonQueryAsync();

                // 3. Valider les changements
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Laisse l'exception remonter pour la gestion dans le contrôleur
            }
        }


    }

}
