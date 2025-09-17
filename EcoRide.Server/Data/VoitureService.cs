using EcoRide.Server.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Text.Json;

public class VoitureService
{
    private readonly string _connectionString;

    public VoitureService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task AjouterVoitureAsync(Voiture voiture)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
            INSERT INTO voiture (
                modele, immatriculation, energie, couleur,
                date_premiere_immatriculation, utilisateur_id, marque_id, nb_place, preference
            ) VALUES (
                @modele, @immatriculation, @energie, @couleur,
                @date_premiere_immatriculation, @utilisateur_id, @marque_id, @nb_place, @preference
            );", connection);

        var preferenceJson = JsonSerializer.Serialize(voiture.preference);

        command.Parameters.AddWithValue("@modele", voiture.modele);
        command.Parameters.AddWithValue("@immatriculation", voiture.immatriculation);
        command.Parameters.AddWithValue("@energie", voiture.energie);
        command.Parameters.AddWithValue("@couleur", voiture.couleur);
        command.Parameters.AddWithValue("@date_premiere_immatriculation", voiture.date_premiere_immatriculation);
        command.Parameters.AddWithValue("@utilisateur_id", voiture.utilisateur_id);
        command.Parameters.AddWithValue("@marque_id", voiture.marque_id);
        command.Parameters.AddWithValue("@nb_place", voiture.nb_place);
        command.Parameters.AddWithValue("@preference", preferenceJson);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Voiture>> GetVoituresByUtilisateurIdAsync(int utilisateurId)
    {
        var result = new List<Voiture>();
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM voiture WHERE utilisateur_id = @uid", connection);
        command.Parameters.AddWithValue("@uid", utilisateurId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader["preference"].ToString() ?? "{}";
            var pref = JsonSerializer.Deserialize<Preference>(json);

            result.Add(new Voiture
            {
                Id = Convert.ToInt32(reader["voiture_id"]),
                modele = reader["modele"].ToString(),
                immatriculation = reader["immatriculation"].ToString(),
                energie = reader["energie"].ToString(),
                couleur = reader["couleur"].ToString(),
                date_premiere_immatriculation = reader["date_premiere_immatriculation"].ToString(),
                utilisateur_id = Convert.ToInt32(reader["utilisateur_id"]),
                marque_id = Convert.ToInt32(reader["marque_id"]),
                nb_place = Convert.ToInt32(reader["nb_place"]),
                preference = pref ?? new Preference()
            });
        }

        return result;
    }

        public async Task<List<Marque>> GetMarqueAsync()
        {
            var result = new List<Marque>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT * FROM marque", connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                
                result.Add(new Marque
                {
                    Id = Convert.ToInt32(reader["marque_id"]),
                    libelle = reader["libelle"].ToString(),
                });
            }

            return result;
        }

    public async Task<int> AjouterOuRecupererMarqueAsync(string libelle)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Vérifie si la marque existe déjà
        var checkCommand = new MySqlCommand("SELECT marque_id FROM marque WHERE LOWER(libelle) = LOWER(@libelle)", connection);
        checkCommand.Parameters.AddWithValue("@libelle", libelle);

        var result = await checkCommand.ExecuteScalarAsync();
        if (result != null)
        {
            return Convert.ToInt32(result);
        }

        // Sinon, insère la nouvelle marque
        // Mise en Title Case avant insertion
        libelle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(libelle.Trim().ToLower());
        var insertCommand = new MySqlCommand("INSERT INTO marque (libelle) VALUES (@libelle); SELECT LAST_INSERT_ID();", connection);
        insertCommand.Parameters.AddWithValue("@libelle", libelle);

        var insertedId = await insertCommand.ExecuteScalarAsync();
        return Convert.ToInt32(insertedId);
    }
}
