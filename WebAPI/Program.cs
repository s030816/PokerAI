using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.VisualBasic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Text;
using Newtonsoft.Json;
using WebAPI;
using WebAPI.Models;



var builder = WebApplication.CreateBuilder(args);

// Add game_services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//string DefaultConnectionString = builder.Configuration.GetConnectionString("CardGameDatabase");


//builder.Services.AddSingleton<MongoClient>(_ => new MongoClient("mongodb://localhost:27017"));



/*
builder.Services.AddSingleton<IMongoDatabase>(
    provider => provider.GetRequiredService<MongoClient>().GetDatabase("CardGames"));
builder.Services.AddSingleton<IMongoCollection<GameState>>(
    provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<GameState>("game_state"));
*/
/*
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ObjectIdJsonConverter());
});
*/
builder.Services.AddScoped<GameStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


// Get all collections


app.MapGet("/gamestate", () => new { Message = "Hello World" , notMessage = "Goodbye"});

//=============================


app.MapGet("/game_state", async (GameStateService game_service)
        => await game_service.GetAll());

app.MapGet("/game_state/{id}", async (GameStateService game_service, string id)
    => await game_service.Get(id));

app.MapPost("/game_state", async (GameStateService game_service, GameState game_state) =>
{
    await game_service.Create(game_state);
    return Results.Created($"/game_state/{game_state._id}", game_state);
});

app.MapPut("/game_state/{id}", async (GameStateService game_service, string id, GameState updateGameState) =>
{
    var game_state = await game_service.Get(id);

    if (game_state is null)
        return Results.NotFound();

    updateGameState._id = game_state._id;

    await game_service.Update(id, updateGameState);

    return Results.NoContent();
});

app.MapDelete("/game_states/{id}", async (GameStateService game_service, string id) =>
{
    var game_state = await game_service.Get(id);

    if (game_state is null) return Results.NotFound();

    await game_service.Delete(id);

    return Results.NotFound();
});



app.MapGet("/companies", () => "asdas");

app.MapGet("/", () => "Hello World!");
System.Diagnostics.Debug.WriteLine("testas");

app.Run("http://localhost:3000");




//public record Office(string Id, Address Address);
//public record Address(string Line1, string Line2, string PostalCode, string Country);


