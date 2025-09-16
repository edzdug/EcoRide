using EcoRide.Server.Data;
using EcoRide.Server.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
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

        public CovoiturageController(CovoiturageService service)
        {
            _service = service;
        }

        [HttpGet("GetItiniraireAll")]
        public async Task<ActionResult<IEnumerable<Covoiturage>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.ToList();
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

        [HttpPost]
        public async Task<ActionResult<Utilisateur>> PostAsync([FromBody] Utilisateur utilisateur)
        {
            if (await _service.EmailExisteAsync(utilisateur.Email))
                return BadRequest("Cet email est déjà utilisé.");

            if (await _service.PseudoExisteAsync(utilisateur.Pseudo))
                return BadRequest("Ce pseudo est déjà utilisé.");

            var insertedId = await _service.AddUtilisateurAsync(utilisateur);

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
    }
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthentificationController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UtilisateurService _service;

        public AuthentificationController(UtilisateurService service , JwtService jwtService)
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
    

}
