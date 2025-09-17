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

    public class Voiture 
    {
        public int? Id { get; set; }
        public string modele { get; set; }
        public string immatriculation { get; set; }
        public string energie { get; set; }
        public string couleur { get; set; }
        public string date_premiere_immatriculation { get; set; }
        public int utilisateur_id { get; set; }
        public int marque_id { get; set; }
        public int nb_place { get; set; }
        public Preference preference { get; set; }
    }

    public class Preference
    {
        public bool fumeur { get; set; }
        public bool animal { get; set; }
        public string autre { get; set; }
    }

    public class Role
    {
        public int? Id { get; set; }
        public string libelle { get; set; }
    }

    public class Possede 
    {
        public string utilisateur_id { get; set; }
        public string role_id { get; set; }
    }

    public class Participation
    {
        public string utilisateur_id { get; set; }
        public string covoiturage_id { get; set; }
    }

    public class Parametre
    {
        public int? id { get; set; }
        public string propriete { get; set; }
        public string valeur { get; set; }
    }

    public class Marque
    {
        public int? Id { get; set; }
        public string libelle { get; set; }
    }

    public class Configuration
    {
        public int? Id { get; set; }
    }

    public class Avis
    {
        public int? Id { get; set; }
        public string commentaire { get; set; }
        public string note { get; set; }
        public string statut { get; set; }

    }
}
