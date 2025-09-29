using EcoRide.Server.Model;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly SmtpSettings _smtp;
    private readonly string _connectionString;
    private readonly CovoiturageService _covoiturageService;

    public EmailService(IOptions<SmtpSettings> smtpOptions, IConfiguration configuration, CovoiturageService covoiturageService)
    {
        _smtp = smtpOptions.Value;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _covoiturageService = covoiturageService;
    }

    public async Task EnvoyerEmailAuxParticipantsAsync(int covoiturageId)
    {
        var participantsEmails = await GetEmailsParticipantsAsync(covoiturageId);
        if (participantsEmails.Count == 0 ) 
            return;

        var covoiturage = await _covoiturageService.GetByIdAsync(covoiturageId);
        if (covoiturage == null)
        {
            Console.WriteLine($"Covoiturage ID {covoiturageId} introuvable.");
            return;
        }

        string corpsMessage = $"\r\nBonjour,\r\n\r\nLe chauffeur a annulé le covoiturage suivant :\r\n\r\n🗓 Date de départ : {covoiturage.DateDepart:dddd dd MMMM yyyy} " +
            $"à {covoiturage.HeureDepart}\r\n📍 Lieu de départ : {covoiturage.LieuDepart}\r\n📍 Lieu d’arrivée : {covoiturage.LieuArrivee}\r\n\r\nVous avez été remboursé automatiquement." +
            $"\r\n\r\nMerci de votre compréhension.\r\n\r\nL’équipe EcoRide.\r\n";

        using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
            EnableSsl = _smtp.EnableSsl
        };

        foreach (var email in participantsEmails)
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(_smtp.Username, "Covoiturage App"),
                Subject = "Annulation de votre covoiturage",
                Body = corpsMessage,
                IsBodyHtml = false
            };

            mail.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d’envoi à {email} : {ex}");
                // Continue l'envoi pour les autres
            }
        }
    }

    // Méthode utilitaire pour récupérer les emails
    private async Task<List<string>> GetEmailsParticipantsAsync(int covoiturageId)
    {
        var emails = new List<string>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
            SELECT u.email 
            FROM participation p
            JOIN utilisateur u ON u.utilisateur_id = p.utilisateur_id
            WHERE p.covoiturage_id = @covoiturageId", connection);

        command.Parameters.AddWithValue("@covoiturageId", covoiturageId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            emails.Add(reader.GetString(0));
        }

        return emails;
    }
}
