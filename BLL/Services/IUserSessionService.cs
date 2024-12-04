namespace WebApplicationZyfra.BLL.Services
{
    using System;
    using System.Threading.Tasks;
    using WebApplicationZyfra.BLL.Repository;
    using WebApplicationZyfra.Data.Entities;

    public interface IUserSessionService
    {
        Task<UserSession> CreateSessionAsync(Guid userId);
        Task<UserSession> GetSessionByIdAsync(Guid sessionId);
        Task<bool> DeleteSessionAsync(Guid sessionId);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public UserSessionService(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        public async Task<UserSession> CreateSessionAsync(Guid userId)
        {
            var newSession = new UserSession
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            await _userSessionRepository.CreateUserSessionAsync(newSession);
            return newSession;
        }

        public async Task<UserSession> GetSessionByIdAsync(Guid sessionId)
        {
            return await _userSessionRepository.GetUserSessionByIdAsync(sessionId);
        }

        public async Task<bool> DeleteSessionAsync(Guid sessionId)
        {
            var session = await _userSessionRepository.GetUserSessionByIdAsync(sessionId);
            if (session == null)
            {
                return false; // Сессия не найдена
            }

            await _userSessionRepository.DeleteUserSessionAsync(sessionId);
            return true;
        }
    }

}
