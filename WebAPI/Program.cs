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
using WebAPI;
using WebAPI.Models;



var builder = WebApplication.CreateBuilder(args);

// Add game_services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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


app.MapGet("/initgame", async (GameStateService game_service) =>
{
    //string id = "641179ed93393afe53ea26d8";
    string id = "6410dc0ebb023e49d13004c4";
    var game_state = await game_service.Get(id);

    if (game_state is null)
        return Results.NotFound();

    var temp = new Game();
    var tmp = temp.new_game();
    tmp._id = id;
    tmp.who_won = temp.check_winner(tmp);
    tmp.winning_hand = temp.winning_combination;

    await game_service.Update(id, tmp);

    return Results.NoContent();
});

app.MapPut("/advance/{id}", async (GameStateService game_service, string id) =>
{

    var game_state = await game_service.Get(id);

    if (game_state is null)
        return Results.NotFound();
    var temp = new Game();
    

    await game_service.Update(id, temp.advance_ingame(game_state));

    return Results.NoContent();
});
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




app.MapGet("/", () => "Hello World!");


/*
int winner = 0;
int counter = 0;
do
{

    var temp = new Game();
    var tmp = temp.new_game();


    winner = temp.check_winner(tmp);
    System.Diagnostics.Debug.WriteLine(winner.ToString() + " " + temp.winning_combination);
} while (++counter < 50);
*/
app.Run("http://localhost:3000");




//public record Office(string Id, Address Address);
//public record Address(string Line1, string Line2, string PostalCode, string Country);


