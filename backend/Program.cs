using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel.Resolution;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));


var app = builder.Build();

var welcome = app.Configuration["AppSettings:WelcomeMessage"];
var version = app.Configuration["AppSettings:Version"];

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Logger.LogInformation("Застосунок запущено. Середовище: {Env}", app.Environment.EnvironmentName);
    

app.MapGet("/", () => "MVP Back-end із SQLite працює!");

app.MapGet("/medias", async(AppDbContext db) => await db.Medias.ToListAsync());

app.MapGet("/medias/{id}", async(int id, AppDbContext db) =>
await db.Medias.FindAsync(id) is Media media
? Results.Ok(media)
: Results.NotFound());


app.MapPost("/medias", async (Media media, AppDbContext db) =>
{
    db.Medias.Add(media);
    await db.SaveChangesAsync();
    return Results.Created($"/medias/{media.Id}", media);
});

app.MapPut("/medias/{Id}", async (int Id, Media input, AppDbContext db) =>
{
    var media = await db.Medias.FindAsync(Id);
    if (media is null) return Results.NotFound();

    media.Name = input.Name;
    media.pagesQuan = input.pagesQuan;
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapDelete("/medias/{Id}", async (int Id, AppDbContext db) =>
{
    var media = await db.Medias.FindAsync(Id);
    if (media is null) return Results.NotFound();

    db.Medias.Remove(media);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();