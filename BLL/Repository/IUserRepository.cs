namespace WebApplicationZyfra.BLL.Repository
{
    using MongoDB.Driver;
    using System;
    using System.Threading.Tasks;
    using WebApplicationZyfra.Data.Entities;

    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(Guid userId);
        Task<User> GetUserByLoginAsync(string username);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _dbContext;

        public UserRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _dbContext.Users.Find(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByLoginAsync(string login)
        {
            return await _dbContext.Users.Find(u => u.Login == login).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _dbContext.Users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _dbContext.Users.ReplaceOneAsync(u => u.UserId == user.UserId, user);
        }
    }

}
