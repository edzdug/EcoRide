using EcoRide.Server.Data;
using EcoRide.Server.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MySql.Data.MySqlClient;
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
        public async Task<ActionResult<IEnumerable<covoiturage>>> Get()
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
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
    }
}
