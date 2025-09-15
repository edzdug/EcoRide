using EcoRide.Server.Model;
using MySql.Data.MySqlClient;
using System.Globalization;

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
                Photo = reader["photo"] as byte[], // ou: (byte[])reader["photo"]
                Pseudo = reader["pseudo"].ToString()
            });

        }

        return utilisateurs;
    }
}
