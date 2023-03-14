using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WebAPI.Models
{
    public record GameState
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("card")]
        public string card { get; set; }

    }
}
