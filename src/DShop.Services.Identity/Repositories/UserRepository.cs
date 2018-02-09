using System;
using System.Threading.Tasks;
using DShop.Services.Identity.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DShop.Services.Identity.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoDatabase _database;
        
        public UserRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<User> GetAsync(Guid id)
            => await Users
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<User> GetAsync(string email)
            => await Users
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant());

        public async Task AddAsync(User user)
            => await Users.InsertOneAsync(user);

        public async Task UpdateAsync(User user)
            => await Users.ReplaceOneAsync(u => u.Id == user.Id, user);

        private IMongoCollection<User> Users 
            => _database.GetCollection<User>("Users");
    }
}