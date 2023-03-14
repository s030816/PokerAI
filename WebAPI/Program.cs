using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string DefaultConnectionString = builder.Configuration.GetConnectionString("CardGameDatabase");


builder.Services.AddSingleton<MongoClient>(_ => new MongoClient("mongodb://localhost:27017"));
builder.Services.AddSingleton<IMongoDatabase>(
    provider => provider.GetRequiredService<MongoClient>().GetDatabase("CardGames"));
builder.Services.AddSingleton<IMongoCollection<GameState>>(
    provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<GameState>("game_state"));

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ObjectIdJsonConverter());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapGet("/gamestate", () => new { Message = "Hello World" , notMessage = "Goodbye"});


/*
// Get the collection
var personsCollection = usersDatabase.GetCollection<User>("users");

// Create an Equality Filter Definition
var personFilter = Builders<User>.Filter
    .Eq(person => person.Id, appPerson.Id);

// Find the document in the collection    
var personFindResult = await personsCollection
    .Find(personFilter).FirstOrDefaultAsync();

 */

app.MapGet("/companies", async (IMongoCollection<GameState> collection)
    => await collection.Find(Builders<GameState>.Filter.Empty).FirstOrDefaultAsync());

app.MapGet("/", () => "Hello World!");

app.Run("http://localhost:3000");

public record GameState(ObjectId Id, string Name, string card);
//public record Office(string Id, Address Address);
//public record Address(string Line1, string Line2, string PostalCode, string Country);

public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
