using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SessionManagementApp
{
    class Program
    {
        static Dictionary<string, (string Password, UserStatus Status)> validUsers = new Dictionary<string, (string, UserStatus)>();
        static Dictionary<string, string> sessions = new Dictionary<string, string>();

        static readonly object dataLock = new object();
        static bool usersDataChanged = false;
        static bool sessionsDataChanged = false;

        enum UserStatus { Available = 0, Busy = 1 }

        static void Main(string[] args)
        {
            LoadValidUsers("users.txt");
            LoadSessions("sessions.txt");

            while (true)
            {
                Console.Write("Введите номер сессии или команду 'delete <sessionId>' для завершения сессии: ");
                string input = Console.ReadLine()?.Trim();

                if (input?.StartsWith("delete ") == true)
                {
                    string sessionId = input.Substring(7);
                    DeleteSession(sessionId);
                }
                else
                {
                    HandleSession(input);
                }

                if (usersDataChanged) SaveValidUsers("users.txt");
                if (sessionsDataChanged) SaveSessions("sessions.txt");
            }
        }

        static void LoadValidUsers(string filename)
        {
            lock (dataLock)
            {
                try
                {
                    validUsers = File.ReadLines(filename)
                        .Select(line => line.Split(','))
                        .Where(parts => parts.Length == 3)
                        .ToDictionary(
                            parts => parts[0].Trim(),
                            parts => (Password: parts[1].Trim(), Status: (UserStatus)int.Parse(parts[2].Trim()))
                        );
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка загрузки пользователей: " + e.Message);
                }
            }
        }

        static void SaveValidUsers(string filename)
        {
            lock (dataLock)
            {
                try
                {
                    File.WriteAllLines(filename, validUsers.Select(kvp => $"{kvp.Key},{kvp.Value.Password},{(int)kvp.Value.Status}"));
                    usersDataChanged = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка сохранения пользователей: " + e.Message);
                }
            }
        }

        static void LoadSessions(string filename)
        {
            lock (dataLock)
            {
                try
                {
                    sessions = File.ReadLines(filename)
                        .Select(line => line.Split(','))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => parts[0].Trim(),
                            parts => parts[1].Trim()
                        );
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка загрузки сессий: " + e.Message);
                }
            }
        }

        static void SaveSessions(string filename)
        {
            lock (dataLock)
            {
                try
                {
                    File.WriteAllLines(filename, sessions.Select(kvp => $"{kvp.Key},{kvp.Value}"));
                    sessionsDataChanged = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка сохранения сессий: " + e.Message);
                }
            }
        }

        static void HandleSession(string sessionId)
        {
            lock (dataLock)
            {
                if (sessions.ContainsKey(sessionId))
                {
                    Console.WriteLine("Вы уже вошли в систему.");
                    return;
                }

                Console.Write("Введите логин: ");
                string username = Console.ReadLine();
                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();

                if (Authenticate(username, password))
                {
                    if (validUsers[username].Status == UserStatus.Busy)
                    {
                        Console.WriteLine("Этот аккаунт уже используется в другой сессии.");
                    }
                    else
                    {
                        string newSessionId = Guid.NewGuid().ToString();
                        sessions[newSessionId] = username;
                        validUsers[username] = (validUsers[username].Password, UserStatus.Busy);
                        usersDataChanged = true;
                        sessionsDataChanged = true;

                        Console.WriteLine($"Успешный вход. Идентификатор сессии: {newSessionId}");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный логин или пароль.");
                }
            }
        }

        static bool Authenticate(string username, string password)
        {
            return validUsers.TryGetValue(username, out var user) && user.Password == password;
        }

        static void DeleteSession(string sessionId)
        {
            lock (dataLock)
            {
                if (sessions.TryGetValue(sessionId, out string username))
                {
                    sessions.Remove(sessionId);
                    validUsers[username] = (validUsers[username].Password, UserStatus.Available);
                    usersDataChanged = true;
                    sessionsDataChanged = true;
                    Console.WriteLine($"Сессия {sessionId} завершена.");
                }
                else
                {
                    Console.WriteLine("Сессия не найдена.");
                }
            }
        }
    }
}
