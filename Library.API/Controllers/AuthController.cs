// =============================================================
// Fichier : Library.API/Controllers/AuthController.cs
// Rôle    : Contrôleur d'authentification.
//           Expose /api/auth/register et /api/auth/login.
// =============================================================

using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserStore _userStore;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserStore userStore, ILogger<AuthController> logger)
        {
            _userStore = userStore;
            _logger = logger;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public IActionResult Register([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Register : {Username}", request.Username);
            var result = _userStore.Register(request);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 401)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login : {Username}", request.Username);
            var result = _userStore.Login(request);
            if (!result.Success)
                return Unauthorized(result);
            return Ok(result);
        }
    }
}