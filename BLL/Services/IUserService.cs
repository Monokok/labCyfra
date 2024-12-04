namespace WebApplicationZyfra.BLL.Services
{
    using System;
    using System.Threading.Tasks;
    using WebApplicationZyfra.BLL.Repository;
    using WebApplicationZyfra.Data.Entities;

    public interface IUserService
    {
        Task<User> RegisterUserAsync(User newUser);
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User?> GetUserByLoginAsync(string login);
        Task<User?> GetUserByIdAsync(Guid id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterUserAsync(User newUser)
        {
            // Хешируем пароль
            var (passwordHash, salt) = PasswordService.HashPassword(newUser.Password);
            newUser.PasswordHash = passwordHash;
            newUser.Salt = salt;

            // Генерируем уникальный UserId и создаем пользователя
            newUser.UserId = Guid.NewGuid();

            await _userRepository.CreateUserAsync(newUser);

            return newUser;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByLoginAsync(username);
            if (user == null)
            {
                return null; // Пользователь не найден
            }

            // Проверяем пароль
            if (PasswordService.VerifyPassword(password, user.PasswordHash))
            {
                return user; // Пароль совпадает
            }

            return null; // Неверный пароль
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            var user = await _userRepository.GetUserByLoginAsync(login);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return user;
        }
    }

}
