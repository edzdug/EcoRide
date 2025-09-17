using EcoRide.Server.Model;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

public class RoleService
{
    private readonly string _connectionString;

    public RoleService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task DefinirRolesUtilisateurAsync(Possede[] roles)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var utilisateurId = roles.First().utilisateur_id;

        // Supprimer les anciens rôles
        var deleteCmd = new MySqlCommand("DELETE FROM possede WHERE utilisateur_id = @uid", connection);
        deleteCmd.Parameters.AddWithValue("@uid", utilisateurId);
        await deleteCmd.ExecuteNonQueryAsync();

        // Ajouter les nouveaux rôles
        foreach (var role in roles)
        {
            var insertCmd = new MySqlCommand(
                "INSERT INTO possede (utilisateur_id, role_id) VALUES (@uid, @rid)", connection);
            insertCmd.Parameters.AddWithValue("@uid", role.utilisateur_id);
            insertCmd.Parameters.AddWithValue("@rid", role.role_id);
            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<Role>> GetRoleAsync()
    {
        var result = new List<Role>();
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM role", connection);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {

            result.Add(new Role
            {
                Id = Convert.ToInt32(reader["role_id"]),
                libelle = reader["libelle"].ToString(),
            });
        }

        return result;
    }
}
