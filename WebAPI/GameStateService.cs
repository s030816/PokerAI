using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebAPI.Models;

namespace WebAPI
{
    public class GameStateService
    {
        private readonly IMongoCollection<GameState> _game_state;

        public GameStateService()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");

            _game_state = mongoClient
                .GetDatabase("CardGame")
                .GetCollection<GameState>("game_state");

        }

        public async Task<List<GameState>> GetAll() =>
            await _game_state.Find(_ => true).ToListAsync();

        public async Task<GameState> Get(string id) =>
            await _game_state.Find(s => s._id == id).FirstOrDefaultAsync();

        public async Task Create(GameState game_state) =>
            await _game_state.InsertOneAsync(game_state);

        public async Task Update(string id, GameState game_state) =>
            await _game_state.ReplaceOneAsync(s => s._id == id, game_state);

        public async Task Delete(string id) =>
            await _game_state.DeleteOneAsync(s => s._id == id);
    }
}
