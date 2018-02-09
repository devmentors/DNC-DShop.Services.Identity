using System;
using System.Threading.Tasks;
using DShop.Services.Identity.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DShop.Services.Identity.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IMongoDatabase _database;

        public RefreshTokenRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<RefreshToken> GetAsync(string token)
            => await RefreshTokens
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Token == token);

        public async Task AddAsync(RefreshToken token)
            => await RefreshTokens.InsertOneAsync(token);

        public async Task UpdateAsync(RefreshToken token)
            => await RefreshTokens.ReplaceOneAsync(x => x.Id == token.Id, token);

        private IMongoCollection<RefreshToken> RefreshTokens 
            => _database.GetCollection<RefreshToken>("RefreshTokens");
    }
}