namespace WebApplicationZyfra.BLL.Repository
{
    using MongoDB.Driver;
    using System;
    using System.Threading.Tasks;
    using WebApplicationZyfra.Data.Entities;

    public interface IUserSessionRepository
    {
        Task<UserSession> GetUserSessionByIdAsync(Guid sessionId);
        Task CreateUserSessionAsync(UserSession userSession);
        Task UpdateUserSessionAsync(UserSession userSession);
        Task DeleteUserSessionAsync(Guid sessionId);
    }

    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly MongoDbContext _dbContext;

        public UserSessionRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserSession> GetUserSessionByIdAsync(Guid sessionId)
        {
            return await _dbContext.UserSessions.Find(us => us.SessionId == sessionId).FirstOrDefaultAsync();
        }

        public async Task CreateUserSessionAsync(UserSession userSession)
        {
            await _dbContext.UserSessions.InsertOneAsync(userSession);
        }

        public async Task UpdateUserSessionAsync(UserSession userSession)
        {
            await _dbContext.UserSessions.ReplaceOneAsync(us => us.SessionId == userSession.SessionId, userSession);
        }

        public async Task DeleteUserSessionAsync(Guid sessionId)
        {
            await _dbContext.UserSessions.DeleteOneAsync(us => us.SessionId == sessionId);
        }
    }

}
