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

}
