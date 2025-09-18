using EcoRide.Server.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using BCrypt.Net;

public class UtilisateurService
{
    private readonly string _connectionString;

    public UtilisateurService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Utilisateur>> GetAllAsync()
    {
        var utilisateurs = new List<Utilisateur>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM utilisateur;", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            utilisateurs.Add(new Utilisateur
            {
                Id = Convert.ToInt32(reader["utilisateur_id"]),
                Nom = reader["nom"].ToString(),
                Prenom = reader["prenom"].ToString(),
                Email = reader["email"].ToString(),
                Password = reader["password"].ToString(),
                Telephone = reader["telephone"].ToString(),
                Adresse = reader["adresse"].ToString(),
                DateNaissance = reader["date_naissance"].ToString(),
                //Photo = reader["photo"] as byte[], // ou: (byte[])reader["photo"]
                Pseudo = reader["pseudo"].ToString()
            });

        }

        return utilisateurs;
    }

    public async Task<Utilisateur?> GetUtilisateurByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM utilisateur WHERE utilisateur_id = @id", connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Utilisateur
            {
                Id = reader.GetInt32("utilisateur_id"),
                Nom = reader.GetString("nom"),
                Prenom = reader.GetString("prenom"),
                Email = reader.GetString("email"),
                Password = reader.GetString("password"),
                Telephone = reader.GetString("telephone"),
                Adresse = reader.GetString("adresse"),
                DateNaissance = reader.GetString("date_naissance"),
                Photo = reader["photo"] is DBNull ? null : Convert.ToBase64String((byte[])reader["photo"]),
                Pseudo = reader.GetString("pseudo")
            };
        }

        return null;
    }

    public async Task<int> AddUtilisateurAsync(Utilisateur utilisateur)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Préparer insertion utilisateur
        var command = new MySqlCommand(@"
        INSERT INTO utilisateur (
            nom, prenom, email, password, telephone,
            adresse, date_naissance, photo, pseudo
        ) VALUES (
            @nom, @prenom, @email, @password, @telephone,
            @adresse, @date_naissance, @photo, @pseudo
        );
        SELECT LAST_INSERT_ID();", connection);

        // Préparer la photo (base64)
        byte[]? photoBytes = null;
        if (!string.IsNullOrEmpty(utilisateur.Photo))
        {
            string base64Data = utilisateur.Photo;
            if (base64Data.Contains(','))
                base64Data = base64Data.Substring(base64Data.IndexOf(',') + 1);

            try
            {
                photoBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                photoBytes = null;
            }
        }

        // Ajouter les paramètres
        command.Parameters.AddWithValue("@nom", utilisateur.Nom);
        command.Parameters.AddWithValue("@prenom", utilisateur.Prenom);
        command.Parameters.AddWithValue("@email", utilisateur.Email);
        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(utilisateur.Password));
        command.Parameters.AddWithValue("@telephone", utilisateur.Telephone);
        command.Parameters.AddWithValue("@adresse", utilisateur.Adresse);
        command.Parameters.AddWithValue("@date_naissance", utilisateur.DateNaissance);
        command.Parameters.AddWithValue("@photo", photoBytes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@pseudo", utilisateur.Pseudo);

        // Exécuter l'insertion utilisateur
        var insertedId = Convert.ToInt32(await command.ExecuteScalarAsync());

        // --- Étape 2 : Créer la configuration ---
        var configCommand = new MySqlCommand(@"
        INSERT INTO configuration (utilisateur_id)
        VALUES (@utilisateurId);
        SELECT LAST_INSERT_ID();", connection);
        configCommand.Parameters.AddWithValue("@utilisateurId", insertedId);
        var configId = Convert.ToInt32(await configCommand.ExecuteScalarAsync());

        // --- Étape 3 : Ajouter le crédit par défaut ---
        var creditCommand = new MySqlCommand(@"
        INSERT INTO parametre (propriete, valeur, configuration_id)
        VALUES ('credit', '20', @configId);", connection);
        creditCommand.Parameters.AddWithValue("@configId", configId);
        await creditCommand.ExecuteNonQueryAsync();

        return insertedId;
    }


    public async Task<bool> EmailExisteAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT COUNT(*) FROM utilisateur WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@Email", email);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> PseudoExisteAsync(string pseudo)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT COUNT(*) FROM utilisateur WHERE pseudo = @Pseudo", connection);
        command.Parameters.AddWithValue("@Pseudo", pseudo);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<Utilisateur?> GetUtilisateurByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM utilisateur WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Utilisateur
            {
                Id = reader.GetInt32("utilisateur_id"),
                Nom = reader.GetString("nom"),
                Prenom = reader.GetString("prenom"),
                Email = reader.GetString("email"),
                Password = reader.GetString("password"),
                Telephone = reader.GetString("telephone"),
                Adresse = reader.GetString("adresse"),
                DateNaissance = reader.GetString("date_naissance"),
                Photo = reader["photo"] is DBNull ? null : Convert.ToBase64String((byte[])reader["photo"]),
                Pseudo = reader.GetString("pseudo")
            };
        }

        return null; // utilisateur non trouvé
    }

    public async Task<decimal> GetCreditAsync(int utilisateurId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        SELECT p.valeur
        FROM parametre p
        JOIN configuration c ON p.configuration_id = c.configuration_id
        WHERE c.utilisateur_id = @userId AND p.propriete = 'credit';", connection);

        command.Parameters.AddWithValue("@userId", utilisateurId);

        var result = await command.ExecuteScalarAsync();

        if (result != null && double.TryParse(result.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double creditDouble))
        {
            return (decimal)creditDouble;
        }

        return 0; // Aucun crédit trouvé
    }

    public async Task RetirerCreditAsync(int utilisateurId, decimal montant)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        UPDATE parametre p
        JOIN configuration c ON p.configuration_id = c.configuration_id
        SET p.valeur = CAST(CAST(p.valeur AS DECIMAL(10,2)) - @montant AS CHAR)
        WHERE c.utilisateur_id = @userId AND p.propriete = 'credit';", connection);

        command.Parameters.AddWithValue("@userId", utilisateurId);
        command.Parameters.AddWithValue("@montant", montant);

        await command.ExecuteNonQueryAsync();
    }


}
