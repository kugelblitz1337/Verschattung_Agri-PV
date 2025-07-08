
using ALKISService.Entities;
using ALKISService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.Geometries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ALKISService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlurstueckController : ControllerBase
    {
        private readonly IFlurstueckService _flurstueckService;
        private readonly IMarkingService _markingService;
        private readonly IZipDownloader _zipDownloader;
        private readonly CoordinateTransformService _coordinateTranformService;
        private static readonly string csvPath = AppDomain.CurrentDomain.BaseDirectory + @"\gemarkungsliste.csv";
        private new List<(Polygon, string, int, int)> _cachedPolygons;

        public FlurstueckController(IFlurstueckService flurstueckService, IMarkingService markingService, IZipDownloader zipDownloader)
        {
            _flurstueckService = flurstueckService;
            _markingService = markingService;
            _zipDownloader = zipDownloader;
        }

        /// <summary>
        /// Importiert alle Flurstücke aus dem XML-Verzeichnis.
        /// </summary>
        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportFlurstueckeAsync()
        {
            //Alle Gemarkungen in die Datenbank importieren
            _markingService.ImportGemarkungenFromCsv(csvPath);
            var markings = _markingService.GetAll();
            var downloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var tasks = new List<Task>();
            foreach (var marking in markings)
            {
                await ProcessMarkingAsync(marking, downloadDir);
            }
            //Alle Gemarkungen holen und 
            return Ok("Import abgeschlossen.");
        }
        private async Task ProcessMarkingAsync(MarkingEntity marking, string downloadDir)
        {
            var markingKey = marking.Gemarkungsschluessel.Trim();
            var markingName = marking.Gemarkung.Trim().Replace(" ", "_");
            var zipname = $"ALKIS-oE_{markingKey}_{markingName}_nas.zip";
            var xmlName = $"ALKIS-oE_{markingKey}_{markingName.Replace("ä", "ae").Replace("Ä", "Ae").Replace("ö", "oe").Replace("Ö", "Oe").Replace("ü", "ue").Replace("Ü", "Ue").Replace("ß", "ss")}_nas.xml";
            var url = $"https://opengeodata.lgl-bw.de/data/alkis/{zipname}";
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }
            var downloadPath = Path.Combine(downloadDir, zipname);

            //Log.WriteLine($"Lade Datei herunter: {url}");
            //Warte auf den Download der ZIP-Datei
            await _zipDownloader.DownloadZipAsync(url, zipname, markingName, markingKey, downloadDir, xmlName);

            await _flurstueckService.ImportFlurstueckFileAsync(Path.Combine(downloadDir, xmlName));
        }
         
        /// <summary>
        /// Liefert alle Flurstücke mit passendem Zähler/nenner.
        /// </summary>
        [HttpGet("getByCounterNominator")]
        [Authorize]
        public IActionResult GetByCounterNominator(string counter, string nominator, string marking)
        {
            var results = _flurstueckService.GetByCounterNominator(counter, nominator, marking);
            return Ok(results);
        }

        /// <summary>
        /// Sucht ein Flurstück, das die Koordinate enthält.
        /// </summary>
        [HttpGet("getByCoordinate")]
        [Authorize]
        public IActionResult GetByCoordinate(double x, double y, string marking)
        {
            var result = _flurstueckService.GetByCoordinate(x, y, marking);
            if (result == null)
                return NotFound("Kein passendes Flurstück gefunden.");
            return Ok(result);
        }

        /// <summary>
        /// Sucht nach Gemarkungen
        /// </summary>
        [HttpGet("getMarkings")]
        [Authorize]
        public IActionResult GetMarkings()
        {
            var result = _markingService.GetAll();
            if (result == null)
                return NotFound("Keine Gemarkungen gefunden!");
            return Ok(result);
        }
        private string GenerateJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("rrDmt0hEZBBzdm8ywewD9UwafMecqtq6vBrm91jsXptWtuctEPbVAVU16YpfZKBt"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, "user-id"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var token = new JwtSecurityToken(
                issuer: "ALKISServiceAPI",
                audience: "frontend",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            if (login.Username == "ALKIS" && login.Password == "ALKIS")
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, login.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    // weitere Claims nach Bedarf
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("arDmt0hEZBBzdm8ywewD9UwafMecqtq6vMrm91jsXptWtuctEPbVAVU16YpfZKBt"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "ALKISServiceAPI",         // gleich wie in deinen Settings!
                    audience: "Frontend",       // gleich wie in deinen Settings!
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(6),
                    signingCredentials: creds);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { token = tokenString });
            }

            return Unauthorized("Login fehlgeschlagen.");
        }
    }

    // Hilfsklasse für die Login-Daten
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}


    

