using System.Reflection.Metadata;

namespace EcoRide.Server.Model
{
    public class Covoiturage
    {
        public int? Id { get; set; }

        public DateTime DateDepart { get; set; }

        public TimeOnly HeureDepart { get; set; }

        public string LieuDepart { get; set; }

        public string DateArrivee { get; set; }

        public string HeureArrivee { get; set; }

        public string LieuArrivee { get; set; }

        public string? Statut { get; set; }

        public int NbPlace { get; set; }

        public decimal PrixPersonne { get; set; }

        public int Voiture_id { get; set; }

    }

    public class CovoiturageDto
    {
        public int? Id { get; set; }
        public DateTime DateDepart { get; set; }
        public TimeOnly HeureDepart { get; set; }
        public string LieuDepart { get; set; }
        public string DateArrivee { get; set; }
        public string HeureArrivee { get; set; }
        public string LieuArrivee { get; set; }
        public string? Statut { get; set; }
        public int NbPlace { get; set; }
        public decimal PrixPersonne { get; set; }
        public int Voiture_id { get; set; }
        public string Energie { get; set; }
        public decimal? NoteMinimale { get; set; }
    }

    public class CovoiturageDetailDto
    {
        public CovoiturageDto Covoiturage { get; set; }
        public List<AvisDto> AvisConducteur { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public Preference PreferenceConducteur { get; set; }
    }

    public class AjouterCovoiturageRequest
    {
        public DateTime DateDepart { get; set; }
        public TimeSpan HeureDepart { get; set; }

        public string LieuDepart { get; set; } 
        public string DateArrivee { get; set; }
        public string HeureArrivee { get; set; }
        public string LieuArrivee { get; set; } 

        public int NbPlace { get; set; }
        public decimal PrixPersonne { get; set; }
        public int VoitureId { get; set; }
    }


    public class Utilisateur {
        public int? Id { get; set; }

        public string Nom { get; set; }

        public string Prenom { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Telephone { get; set; }

        public string Adresse { get; set; }

        public string DateNaissance { get; set; }

        public string? Photo { get; set; }

        public string Pseudo { get; set; }
    }

    public class UtilisateurDto
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Adresse { get; set; }
        public string DateNaissance { get; set; }
        public string? Photo { get; set; }
        public string Pseudo { get; set; }
    }

    public class Voiture 
    {
        public int? Id { get; set; }
        public string Modele { get; set; }
        public string Immatriculation { get; set; }
        public string Energie { get; set; }
        public string Couleur { get; set; }
        public string Date_premiere_immatriculation { get; set; }
        public int Utilisateur_id { get; set; }
        public int Marque_id { get; set; }
        public int Nb_place { get; set; }
        public Preference Preference { get; set; }
    }

    public class Preference
    {
        public bool Fumeur { get; set; }
        public bool Animal { get; set; }
        public string? Autre { get; set; }
    }

    public class Role
    {
        public int? Id { get; set; }
        public string Libelle { get; set; }
    }

    public class Possede 
    {
        public string Utilisateur_id { get; set; }
        public string Role_id { get; set; }
    }

    public class Participation
    {
        public int Utilisateur_id { get; set; }
        public int Covoiturage_id { get; set; }
    }

    public class ParticipationRequest
    {
        public int UtilisateurId { get; set; }
        public int CovoiturageId { get; set; }
    }


    public class Parametre
    {
        public int? id { get; set; }
        public string Propriete { get; set; }
        public string Valeur { get; set; }
        public int Configuration_id { get; set; }
    }

    public class Marque
    {
        public int? Id { get; set; }
        public string Libelle { get; set; }
    }

    public class Configuration
    {
        public int? Id { get; set; }
        public int Utilisateur_id { get; set; }
    }

    public class Avis
    {
        public int? Id { get; set; }
        public string? Commentaire { get; set; }
        public int Note { get; set; }
        public string Statut { get; set; }

    }
    public class AvisDto
    {
        public string? Note { get; set; }
        public string? Commentaire { get; set; }
    }

    public class TempAvis
    {
        public Avis avis { get; set; }
        public int utilisateur_id { get; set; }
        public int covoiturage_id { get; set; }
    }

    public class Depose
    {
        public int Utilisateur_id { get; set; }
        public int Avis_id { get; set; }
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }


}
