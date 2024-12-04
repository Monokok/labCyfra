using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

using WebApplicationZyfra.BLL;
using WebApplicationZyfra.BLL.Services;
using WebApplicationZyfra.Controllers.models;
using WebApplicationZyfra.Data.Entities;

namespace WebApplicationZyfra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;
        private readonly IUserSessionService _userSessionService;


        private static ConcurrentDictionary<string, string> Sessions = new();
        private static ConcurrentDictionary<string, string> Users = new();
        
        private static readonly string AdminLogin = "admin";
        private static readonly string AdminPassword = "admin";
        public UsersController(ILogger<UsersController> logger, IUserService userService, IUserSessionService sessionService)
        {
            _logger = logger;
            _userService= userService;
            _userSessionService= sessionService;

        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            // Àóòåíòèôèêàöèÿ ïîëüçîâàòåëÿ
            var user = await _userService.AuthenticateUserAsync(request.Login, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Ïðîâåðêà íà óæå ñóùåñòâóþùóþ àêòèâíóþ ñåññèþ
            var existingSession = await _userSessionService.GetSessionByIdAsync(user.UserId);
            if (existingSession != null)
            {
                return Ok(new
                {
                    Message = "User already logged in",
                    Session = existingSession
                });
            }

            // Ñîçäàíèå íîâîé ñåññèè
            var newSession = await _userSessionService.CreateSessionAsync(user.UserId);

            return Ok(new
            {
                Message = "Login successful",
                Session = newSession
            });
        }


        [HttpGet("session/{id}")]
        public async Task<IActionResult> CheckSessionAsync(string id)
        {
            // Ïðîâåðÿåì, ÿâëÿåòñÿ ëè `id` êîððåêòíûì GUID
            if (!Guid.TryParse(id, out var sessionId))
            {
                return BadRequest("Invalid session ID format.");
            }

            // Èùåì ñåññèþ â áàçå äàííûõ
            var session = await _userSessionService.GetSessionByIdAsync(sessionId);

            if (session != null && session.ExpiresAt > DateTime.UtcNow)
            {
                return Ok("Âû óæå âîøëè â ñèñòåìó.");
            }

            return Unauthorized("Ñåññèÿ íå íàéäåíà èëè èñòåêëà.");
        }

        [HttpDelete("session/{id}")]
        public async Task<IActionResult> DeleteSessionAsync(string id)
        {
            // Ïðîâåðÿåì êîððåêòíîñòü ôîðìàòà ID
            if (!Guid.TryParse(id, out var sessionId))
            {
                return BadRequest("Invalid session ID format.");
            }

            // Óäàëÿåì ñåññèþ èç áàçû äàííûõ
            var deleted = await _userSessionService.DeleteSessionAsync(sessionId);

            if (deleted)
            {
                return Ok("Ñåññèÿ óäàëåíà.");
            }

            return NotFound("Ñåññèÿ íå íàéäåíà.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            // Ïðîâåðêà íà êîððåêòíîñòü âõîäíûõ äàííûõ
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Login and password are required.");
            }

            // Ïðîâåðÿåì, ñóùåñòâóåò ëè ïîëüçîâàòåëü ñ òàêèì ëîãèíîì
            var existingUser = await _userService.GetUserByLoginAsync(request.Login);
            if (existingUser != null)
            {
                return Conflict($"User with login '{request.Login}' already exists.");
            }

            User newUser = new User
            {
                Login = request.Login,
                Password = request.Password, // Õýøèðóåòñÿ â _userService.RegisterUserAsync
            };
            try
            {
                var registeredUser = await _userService.RegisterUserAsync(newUser);
                return CreatedAtAction(nameof(GetUserById), new { id = registeredUser.UserId }, registeredUser);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // Ìåòîä äëÿ ïîëó÷åíèÿ ïîëüçîâàòåëÿ ïî ID
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpDelete("admin/session/{id}")]
        public async Task<IActionResult> AdminDeleteSessionAsync(string id, [FromBody] AdminLoginRequest adminRequest)

        {
            if (adminRequest.Login != AdminLogin || adminRequest.Password != AdminPassword)
            {
                return Unauthorized("Íåâåðíûå ëîãèí èëè ïàðîëü àäìèíèñòðàòîðà.");
            }
            // Ïðîâåðêà ôîðìàòà ID
            if (!Guid.TryParse(id, out var sessionId))
            {
                return BadRequest("Invalid session ID format.");
            }

            // Óäàëåíèå ñåññèè
            var deleted = await _userSessionService.DeleteSessionAsync(sessionId);

            if (deleted)
            {
                return Ok($"Ñåññèÿ ñ ID {id} óäàëåíà àäìèíèñòðàòîðîì.");
            }

            return NotFound("Ñåññèÿ íå íàéäåíà.");
        }
    }
}
