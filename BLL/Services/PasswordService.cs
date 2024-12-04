namespace WebApplicationZyfra.BLL.Services
{
    public class PasswordService
    {
        // Метод для хеширования пароля
        public static (string passwordHash, string salt) HashPassword(string password)
        {
            // Генерация соли
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            // Хеширование пароля с использованием соли
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return (passwordHash, salt);
        }

        // Метод для проверки пароля
        public static bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash); // Проверка пароля
        }
    }
}
