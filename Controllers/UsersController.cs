using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace WebApplicationZyfra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private static ConcurrentDictionary<string, string> Sessions = new();
        private static ConcurrentDictionary<string, string> Users = new();

        private static readonly string UsersFilePath = "Users.txt";
        private static readonly object FileLock = new object(); //для записи новых пользователей при регистрации
        private static readonly string AdminLogin = "admin";
        private static readonly string AdminPassword = "admin";
        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
            if (!System.IO.File.Exists(UsersFilePath))
            {
                System.IO.File.Create(UsersFilePath).Dispose();
            }
            var lines = System.IO.File.ReadAllLines(UsersFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(",");
                Users.TryAdd(parts[0], parts[1]); // Логин-пароль
            }
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (Users.TryGetValue(request.Login, out var password) && password == request.Password)
            {
                var sessionId = Guid.NewGuid().ToString();
                Sessions.TryAdd(sessionId, request.Login); // Добавляем новую сессию
                return Ok(new { SessionId = sessionId });
            }
            return Unauthorized("Invalid login or password.");
        }

        [HttpGet("session/{id}")]
        public IActionResult CheckSession(string id)
        {
            if (Sessions.ContainsKey(id))
                return Ok("Вы уже вошли в систему.");
            return Unauthorized("Сессия не найдена.");
        }

        [HttpDelete("session/{id}")]
        public IActionResult DeleteSession(string id)
        {
            if (Sessions.TryRemove(id, out _))
                return Ok("Сессия удалена.");
            return NotFound("Сессия не найдена.");
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (Users.ContainsKey(request.Login))
            {
                return Conflict("Пользователь с таким логином уже существует.");
            }

            // добавление в память
            if (Users.TryAdd(request.Login, request.Password))
            {
                // добавление в файл
                lock (FileLock) // синхронизация записи в файл
                {
                    System.IO.File.AppendAllText(UsersFilePath, $"{request.Login},{request.Password}{Environment.NewLine}");
                }
                return Ok("Регистрация прошла успешно.");
            }

            return BadRequest("Не удалось зарегистрировать пользователя.");
        }

        [HttpDelete("admin/session/{id}")]
        public IActionResult AdminDeleteSession(string id, [FromBody] AdminLoginRequest adminRequest)
        {
            if (adminRequest.Login != AdminLogin || adminRequest.Password != AdminPassword)
            {
                return Unauthorized("Неверные логин или пароль администратора.");
            }

            if (Sessions.TryRemove(id, out _))
            {
                return Ok($"Сессия с ID {id} удалена администратором.");
            }
            return NotFound("Сессия не найдена.");
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
    public class AdminLoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
