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

        var command = new MySqlCommand(@"
        INSERT INTO utilisateur (
            nom, prenom, email, password, telephone,
            adresse, date_naissance, photo, pseudo
        ) VALUES (
            @nom, @prenom, @email, @password, @telephone,
            @adresse, @date_naissance, @photo, @pseudo
        );
        SELECT LAST_INSERT_ID();", connection);

        // Nettoyer la chaîne base64 (pour les data:image/png;base64,...)
        byte[]? photoBytes = null;

        if (!string.IsNullOrEmpty(utilisateur.Photo))
        {
            string base64Data = utilisateur.Photo;
            if (base64Data.Contains(','))
            {
                base64Data = base64Data.Substring(base64Data.IndexOf(',') + 1);
            }

            try
            {
                photoBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                photoBytes = null; // ou tu peux gérer l'erreur plus finement
            }
        }

        command.Parameters.AddWithValue("@nom", utilisateur.Nom);
        command.Parameters.AddWithValue("@prenom", utilisateur.Prenom);
        command.Parameters.AddWithValue("@email", utilisateur.Email); 
        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(utilisateur.Password));
        command.Parameters.AddWithValue("@telephone", utilisateur.Telephone);
        command.Parameters.AddWithValue("@adresse", utilisateur.Adresse);
        command.Parameters.AddWithValue("@date_naissance", utilisateur.DateNaissance);
        command.Parameters.AddWithValue("@photo", photoBytes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@pseudo", utilisateur.Pseudo);

        var insertedId = Convert.ToInt32(await command.ExecuteScalarAsync());
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


}
