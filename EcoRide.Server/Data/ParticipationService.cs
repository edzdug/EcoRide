using EcoRide.Server.Model;
using MySqlConnector;
using System.Globalization;
using System.Text.Json;

namespace EcoRide.Server.Data
{
    public class ParticipationService
    {
        private readonly string _connectionString;

        public ParticipationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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


    }
}
