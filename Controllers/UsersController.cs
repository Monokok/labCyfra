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
        private static readonly object FileLock = new object(); //��� ������ ����� ������������� ��� �����������
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
                Users.TryAdd(parts[0], parts[1]); // �����-������
            }
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (Users.TryGetValue(request.Login, out var password) && password == request.Password)
            {
                var sessionId = Guid.NewGuid().ToString();
                Sessions.TryAdd(sessionId, request.Login); // ��������� ����� ������
                return Ok(new { SessionId = sessionId });
            }
            return Unauthorized("Invalid login or password.");
        }

        [HttpGet("session/{id}")]
        public IActionResult CheckSession(string id)
        {
            if (Sessions.ContainsKey(id))
                return Ok("�� ��� ����� � �������.");
            return Unauthorized("������ �� �������.");
        }

        [HttpDelete("session/{id}")]
        public IActionResult DeleteSession(string id)
        {
            if (Sessions.TryRemove(id, out _))
                return Ok("������ �������.");
            return NotFound("������ �� �������.");
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (Users.ContainsKey(request.Login))
            {
                return Conflict("������������ � ����� ������� ��� ����������.");
            }

            // ���������� � ������
            if (Users.TryAdd(request.Login, request.Password))
            {
                // ���������� � ����
                lock (FileLock) // ������������� ������ � ����
                {
                    System.IO.File.AppendAllText(UsersFilePath, $"{request.Login},{request.Password}{Environment.NewLine}");
                }
                return Ok("����������� ������ �������.");
            }

            return BadRequest("�� ������� ���������������� ������������.");
        }

        [HttpDelete("admin/session/{id}")]
        public IActionResult AdminDeleteSession(string id, [FromBody] AdminLoginRequest adminRequest)
        {
            if (adminRequest.Login != AdminLogin || adminRequest.Password != AdminPassword)
            {
                return Unauthorized("�������� ����� ��� ������ ��������������.");
            }

            if (Sessions.TryRemove(id, out _))
            {
                return Ok($"������ � ID {id} ������� ���������������.");
            }
            return NotFound("������ �� �������.");
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
