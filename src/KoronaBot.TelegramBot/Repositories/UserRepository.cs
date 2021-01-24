using KoronaBot.TelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronaBot.TelegramBot.Repositories
{
    public class UserRepository : IDisposable, IUserRepository
    {
        private readonly string collectionString = nameof(UserRepository);
        private readonly LiteDB.LiteDatabase database;
        private readonly LiteDB.ILiteCollection<UserData> userCollection;
        public UserRepository()
        {
            this.database = new LiteDB.LiteDatabase("users.json");
            this.userCollection = this.database.GetCollection<UserData>(collectionString);
        }

        public Task<List<UserData>> GetUsers()
        {
            return Task.FromResult(this.userCollection.FindAll().ToList());
        }

        public Task DeleteUser(UserData data)
        {
            this.userCollection.DeleteMany(x => x.UserId.Equals(data.UserId));
            this.database.Commit();
            this.database.Checkpoint();
            return Task.CompletedTask;
        }

        public Task UpsertUser(UserData data)
        {
            this.userCollection.Upsert(data.UserId, data);
            this.database.Commit();
            this.database.Checkpoint();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.database.Dispose();
        }

        public async Task<UserData> GetUser(string userId)
            => (await this.GetUsers()).Find(x => x.UserId == userId);
    }
}
