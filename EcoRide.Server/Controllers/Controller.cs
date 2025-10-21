using EcoRide.Server.Data;
using EcoRide.Server.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
using System.Text.Json;
/*
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var users = _context.Users.ToList();
        return Ok(users);
    }
}*/

namespace EcoRide.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItineraireController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ItineraireController> _logger;
        private readonly AppDbContext _context;

        public ItineraireController(ILogger<ItineraireController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("GetItiniraireAll")]
        public async Task<ActionResult<IEnumerable<Covoiturage>>> Get()
        {
            /*
            return Enumerable.Range(1, 5).Select(index => new Covoiturage
            {
                DateDepart = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                heureDepart = TimeOnly.FromDateTime(DateTime.Now),
                DateArrivee = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                heureArrivee = TimeOnly.FromDateTime(DateTime.Now).ToString(),
                Statut = Summaries[Random.Shared.Next(Summaries.Length)],
                NbPlace = Random.Shared.Next(0, 4),
                PrixPersonne = Random.Shared.Next(10,100)
            })
            .ToArray();*/

            return _context.covoiturage.ToArray();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CovoiturageController : ControllerBase
    {
        private readonly CovoiturageService _service;
        private readonly UtilisateurService _serviceUser;
        private readonly ParticipationService _serviceParticipation;
        private readonly EmailService _emailService;
        private readonly VoitureService _voitureService;

        public CovoiturageController(CovoiturageService service, UtilisateurService serviceUser, 
            ParticipationService serviceParticipation, EmailService emailService, VoitureService voitureService)
        {
            _service = service;
            _serviceUser = serviceUser;
            _serviceParticipation = serviceParticipation;
            _emailService = emailService;
            _voitureService = voitureService;
        }

        [HttpGet("GetItiniraireAll")]
        public async Task<ActionResult<IEnumerable<CovoiturageDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.ToList();
        }

        [HttpGet("GetDetail/{id}")]
        public async Task<ActionResult<CovoiturageDetailDto>> GetDetail(int id)
        {
            var detail = await _service.GetDetailAsync(id);
            if (detail == null) return NotFound();

            return Ok(detail);
        }

        [HttpPost("Participer")]
        public async Task<IActionResult> Participer([FromBody] ParticipationRequest request)
        {
            var utilisateur = await _serviceUser.GetUtilisateurByIdAsync(request.UtilisateurId);
            var covoiturage = await _service.GetByIdAsync(request.CovoiturageId);
            var credit = await _serviceUser.GetCreditAsync(request.UtilisateurId);

            if (utilisateur == null || covoiturage == null)
                return NotFound("Utilisateur ou trajet introuvable.");

            if (covoiturage.NbPlace <= 0)
                return BadRequest("Plus de places disponibles.");

            if (credit < covoiturage.PrixPersonne)
                return BadRequest("Crédit insuffisant.");

            if (await _serviceParticipation.ExisteParticipationAsync(request.UtilisateurId, request.CovoiturageId))
                return BadRequest("Déjà inscrit à ce covoiturage.");

            // Enregistrement
            await _serviceParticipation.AjouterParticipationAsync(request.UtilisateurId, request.CovoiturageId);
            await _serviceUser.RetirerCreditAsync(request.UtilisateurId, covoiturage.PrixPersonne);
            await _service.DecrementerPlaceAsync(request.CovoiturageId);

            return Ok("Participation enregistrée !");
        }

        [HttpPost("Ajouter")]
        public async Task<IActionResult> Ajouter([FromBody] AjouterCovoiturageRequest request,[FromQuery] int utilisateurId )
        {
            try
            {
                await _service.AjouterCovoiturageAsync(request);
                await _serviceUser.RetirerCreditAsync(utilisateurId, 2);
                return Ok(new { message = "Covoiturage ajouté avec succès !" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l’ajout : {ex.Message}");
            }
        }

        [HttpGet("GetHistorique/{id}")]
        public async Task<ActionResult<List<Covoiturage>>> GetHistorique(int id)
        {
            var covoiturages = new List<Covoiturage>();

            // 1. Récupérer les covoiturages en tant que passager
            var participations = await _serviceParticipation.RechercheParticipationAsync(id);
            foreach (var covoiturageId in participations)
            {
                var covoiturage = await _service.GetByIdAsync(covoiturageId);
                if (covoiturage != null)
                {
                    covoiturages.Add(covoiturage);
                }
            }

            // 2. Récupérer toutes les voitures de l'utilisateur (chauffeur)
            var voitures = await _voitureService.GetVoituresByUtilisateurIdAsync(id);
            var voitureIds = voitures.Select(v => v.Id).ToList();

            // 3. Récupérer tous les covoiturages créés avec ces voitures
            foreach (var voitureId in voitureIds)
            {
                var covoituragesChauffeur = await _service.GetByVoitureIdAsync((int)voitureId);
                covoiturages.AddRange(covoituragesChauffeur);
            }

            // 4. Supprimer les doublons (ex: s'il est à la fois passager et conducteur d’un même trajet)
            var covoituragesUniques = covoiturages
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            // 5. Trier du plus récent au plus ancien (optionnel)
            var sorted = covoituragesUniques.OrderByDescending(c => c.DateDepart).ToList();

            return Ok(sorted);
        }


        [HttpDelete("Annuler/{covoiturageId}/utilisateur/{utilisateurId}")]
        public async Task<IActionResult> Annuler(int covoiturageId, int utilisateurId)
        {
            try
            {
                var covoiturage = await _service.GetByIdAsync(covoiturageId);
                if (covoiturage == null)
                    return NotFound("Covoiturage non trouvé.");

                // Vérifie si c'est le chauffeur
                var isChauffeur = await _serviceUser.IsChauffeurAsync(covoiturageId, utilisateurId);

                if (isChauffeur)
                {
                    // 1. Annulation par le chauffeur => supprimer le covoiturage
                    await _emailService.EnvoyerEmailAuxParticipantsAsync(covoiturageId);
                    await _service.AnnulerCovoiturageParChauffeurAsync(covoiturageId, utilisateurId);
                }
                else
                {
                    // 2. Annulation par un participant
                    await _serviceParticipation.AnnulerParticipationAsync(covoiturageId, utilisateurId);
                    await _serviceUser.RembourserCreditAsync(utilisateurId, utilisateurId); 
                }

                return Ok(new { message = "Annulation effectuée avec succès." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur : {ex.Message}");
            }
        }

        // Vérifie si l'utilisateur est chauffeur du covoiturage
        [HttpGet("isChauffeur/{covoiturageId}/utilisateur/{utilisateurId}")]
        public async Task<IActionResult> IsChauffeur( int covoiturageId, int utilisateurId)
        {
            bool isChauffeur = await _serviceUser.IsChauffeurAsync(covoiturageId, utilisateurId);
            return Ok(new { isChauffeur });
        }

        // Démarrer le covoiturage
        [HttpPost("Demarrer")]
        public async Task<IActionResult> DemarrerCovoiturage([FromBody] int covoiturageId)
        {
            try
            {
                // Vérifie que le covoiturage existe 
                var covoiturage = await _service.GetByIdAsync(covoiturageId);
                if (covoiturage == null)
                    return NotFound($"Covoiturage avec l'ID {covoiturageId} non trouvé.");

                // Mise à jour du statut
                await _service.updateStateCovoiturage(covoiturageId, "en_cours");

                return Ok(new { message = "Covoiturage démarré avec succès." });
            }
            catch (Exception ex)
            {
                // Log si tu as un logger
                return StatusCode(500, new { message = "Erreur serveur", details = ex.Message });
            }
        }

        // Arriver à destination
        [HttpPost("Arriver")]
        public async Task<IActionResult> ArriverCovoiturage([FromBody] int covoiturageId)
        {
            try
            {
                // Vérifie que le covoiturage existe 
                var covoiturage = await _service.GetByIdAsync(covoiturageId);
                if (covoiturage == null)
                    return NotFound($"Covoiturage avec l'ID {covoiturageId} non trouvé.");

                // Mise à jour du statut
                await _service.updateStateCovoiturage(covoiturageId, "arriver");
                await _emailService.EnvoyerEmailRetourAvisAsync(covoiturageId);

                return Ok(new { message = "Covoiturage arrivé avec succès." });
            }
            catch (Exception ex)
            {
                // Log si tu as un logger
                return StatusCode(500, new { message = "Erreur serveur", details = ex.Message });
            }
        }

        [HttpGet("statistiques-par-jour")]
        public async Task<ActionResult<IEnumerable<CovoiturageStatistiqueParJour>>> GetStatsParJour([FromQuery] int year, [FromQuery] int month)
        {
            var stats = await _service.GetCovoituragesParJourAsync(year, month);
            return Ok(stats);
        }

        [HttpGet("statistiques-par-jour/credits")]
        public async Task<ActionResult<List<CreditsStatistiqueParJour>>> GetCreditsParJour([FromQuery] int year, [FromQuery] int month)
        {
            var stats = await _service.GetCreditsParJourAsync(year, month);
            return Ok(stats);
        }

        [HttpGet("total-mensuel")]
        public async Task<ActionResult<int>> GetTotalCreditsDuMois([FromQuery] int year, [FromQuery] int month)
        {
            var total = await _service.GetTotalCreditsMoisAsync(year, month);
            return Ok(total);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateurController : ControllerBase
    {
        private readonly UtilisateurService _service;

        public UtilisateurController(UtilisateurService service)
        {
            _service = service;
        }

        [HttpGet("GetUtilisateurAll")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Utilisateur>> GetById(int id)
        {
            var user = await _service.GetUtilisateurByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("PostUser")]
        public async Task<ActionResult<Utilisateur>> PostAsync([FromBody] Utilisateur utilisateur, [FromQuery] string statut)
        {
            if (await _service.EmailExisteAsync(utilisateur.Email))
                return BadRequest("Cet email est déjà utilisé.");

            if (await _service.PseudoExisteAsync(utilisateur.Pseudo))
                return BadRequest("Ce pseudo est déjà utilisé.");

            try { 
            var insertedId = await _service.AddUtilisateurAsync(utilisateur, statut);

            var utilisateurDto = new UtilisateurDto
            {
                Id = insertedId,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Email = utilisateur.Email,
                Telephone = utilisateur.Telephone,
                Adresse = utilisateur.Adresse,
                DateNaissance = utilisateur.DateNaissance,
                Photo = utilisateur.Photo,
                Pseudo = utilisateur.Pseudo
            };

            // CreatedAtAction nécessite que tu aies une méthode "GetById" accessible
            return CreatedAtAction(nameof(GetById), new { id = insertedId }, utilisateurDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] PostUser failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("modifierAutorisation/{id}")]
        public async Task<IActionResult> ModifierAutorisation(int id, [FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("autorisation", out var autorisationElement))
                return BadRequest("Champ 'autorisation' requis.");

            var nouvelleAutorisation = autorisationElement.GetString();

            await _service.UpdateAutorisationAsync(id, nouvelleAutorisation); // implémente cette méthode

            return NoContent();
        }


    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthentificationController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UtilisateurService _service;

        public AuthentificationController(UtilisateurService service, JwtService jwtService)
        {
            _service = service;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var utilisateur = await _service.GetUtilisateurByEmailAsync(request.Email);
            if (utilisateur == null)
                return Unauthorized("Email ou mot de passe incorrect");

            if (utilisateur.Autorisation != "actif")
                return Unauthorized("Le compte actuel est suspendu ou bloqué");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, utilisateur.Password);
            if (!isPasswordValid)
                return Unauthorized("Email ou mot de passe incorrect");

            var token = _jwtService.GenerateToken(utilisateur); // méthode pour générer JWT

            return Ok(new LoginResponse { Token = token, User = utilisateur });
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public Utilisateur User { get; set; }
        }

    }


    [ApiController]
    [Route("api/[controller]")]
    public class ProfilController : ControllerBase
    {
        private readonly UtilisateurService _utilisateurService;
        private readonly RoleService _roleService;
        private readonly VoitureService _voitureService;

        public ProfilController(IConfiguration config)
        {
            _utilisateurService = new UtilisateurService(config);
            _roleService = new RoleService(config);
            _voitureService = new VoitureService(config);
        }

        // GET: /api/profil/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Utilisateur>> GetUtilisateur(int id)
        {
            var user = await _utilisateurService.GetUtilisateurByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST: /api/profil/roles
        [HttpPost("roleSet")]
        public async Task<IActionResult> SetRoles([FromBody] Possede[] roles)
        {
            await _roleService.DefinirRolesUtilisateurAsync(roles);
            return Ok();
        }

        [HttpGet("roleGet")]
        public async Task<ActionResult<List<Role>>> GetRoleAll()
        {
            var roles = await _roleService.GetRoleAsync();
            return Ok(roles);
        }

        // POST: /api/profil/voiture
        [HttpPost("voiture")]
        public async Task<IActionResult> AjouterVoiture([FromBody] Voiture voiture)
        {
            await _voitureService.AjouterVoitureAsync(voiture);
            return Ok();
        }

        // GET: /api/profil/voiture/{utilisateurId}
        [HttpGet("voiture/{utilisateurId}")]
        public async Task<ActionResult<List<Voiture>>> GetVoitures(int utilisateurId)
        {
            var voitures = await _voitureService.GetVoituresByUtilisateurIdAsync(utilisateurId);
            return Ok(voitures);
        }

        [HttpGet("marqueGet")]
        public async Task<ActionResult<List<Voiture>>> GetMarqueAll()
        {
            var marques = await _voitureService.GetMarqueAsync();
            return Ok(marques);
        }

        [HttpPost("marqueAdd")]
        public async Task<ActionResult<int>> AjouterMarque([FromBody] string libelle)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                return BadRequest("Libellé de marque invalide.");

            var id = await _voitureService.AjouterOuRecupererMarqueAsync(libelle);
            return Ok(id);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AvisController : ControllerBase
    {
        private readonly AvisService _service;
        private readonly UtilisateurService _utilisateurService;

        public AvisController(AvisService service, UtilisateurService utilisateurService)
        {
            _service = service;
            _utilisateurService = utilisateurService;
        }

        [HttpPost("Envoyer/{utilisateurId}/{covoiturageId}")]
        public async Task<ActionResult<Avis>> PostAvisAsync([FromBody] AvisDto dto, int utilisateurId, int covoiturageId)
        {
            try
            {
                await _service.EnvoieAvisAsync(dto, covoiturageId, utilisateurId);
                return Ok(new { message = "Avis ajouté avec succès dans temp_avis." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'ajout de l'avis :", ex.Message);
                return StatusCode(500, "Une erreur est survenue lors de l'enregistrement de l'avis.");
            }
        }

        [HttpGet("Get/temp_avis")]
        public async Task<IActionResult> GetAvisEnAttente()
        {
            var result = await _service.GetAvisEnAttenteAsync();
            return Ok(result);
        }

        [HttpGet("Get/Refuse/temp_avis")]
        public async Task<IActionResult> GetAvisRefuse()
        {
            var result = await _service.GetAvisRefuseAsync();
            return Ok(result);
        }

        [HttpPost("Valider/temp_avis")]
        public async Task<IActionResult> ValiderAvisEnAttente([FromBody] TempAvis temp_avis)
        {
            try
            {
                await _service.VerifAvisAsync(temp_avis, "valide");
                await _utilisateurService.AjouterCreditChauffeurAsync(temp_avis.covoiturage_id);
                return Ok(new { message = "Etat modifié avec succès en 'valide'." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la modification de l'état :", ex.Message);
                return StatusCode(500, "Une erreur est survenue lors de la modification de l'état.");
            }
        }

        [HttpPost("Refuser/temp_avis")]
        public async Task<IActionResult> RefuserAvisEnAttente([FromBody] TempAvis temp_avis)
        {
            try
            {
                await _service.VerifAvisAsync(temp_avis, "refuse");
                return Ok(new { message = "Etat modifié avec succès en 'refuse'." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la modification de l'état :", ex.Message);
                return StatusCode(500, "Une erreur est survenue lors de la modification de l'état.");
            }
        }

        [HttpPost("temp_avis_refuse/validation")]
        public async Task<IActionResult> ValiderAvisRefuse([FromBody] ProblemeAvis pb_avis)
        {
            try
            {
                await _service.ValidePbAvisAsync(pb_avis);
                await _utilisateurService.AjouterCreditChauffeurAsync((int)pb_avis.covoiturage.Id);
                return Ok(new { message = "Etat modifié avec succès en 'valide'." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la modification de l'état :", ex.Message);
                return StatusCode(500, "Une erreur est survenue lors de la modification de l'état.");
            }
        }
    }

}
