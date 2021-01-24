using KoronaBot.TelegramBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KoronaBot.TelegramBot.Repositories
{
    public interface IUserRepository
    {
        Task DeleteUser(UserData data);
        Task<List<UserData>> GetUsers();

        Task<UserData?> GetUser(string userId);
        Task UpsertUser(UserData data);
    }
}