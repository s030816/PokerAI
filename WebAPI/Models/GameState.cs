using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WebAPI.Models
{
    public record GameState
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }

        [BsonElement("state")]
        public int? state { get; set; }

        [BsonElement("deck")]
        public List<string>? deck { get; set; }

        [BsonElement("player_hand")]
        public List<string>? player_hand { get; set; }

        [BsonElement("opponent_hand")]
        public List<string>? opponent_hand { get; set; }

        [BsonElement("opponent_bank")]
        public int opponent_bank { get; set; }

        [BsonElement("player_bank")]
        public int player_bank { get; set; }

        [BsonElement("opponent_bet")]
        public int opponent_bet { get; set; }

        [BsonElement("player_bet")]
        public int player_bet { get; set; }

    }
}
