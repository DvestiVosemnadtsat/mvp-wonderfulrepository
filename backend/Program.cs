using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.Win32;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme
)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MVP Back-end API",
        Version = "v1",
        Description = "Документація  REST API навчального MVP-проекту"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT - токен, отриманий з ендпоінта / auth / login."
    });


    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
{
{ new OpenApiSecuritySchemeReference("Bearer", doc), new
List<string>() }
});
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
options.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP Back-End API v1");
options.DocumentTitle = "MVP Back-End API";
});

app.UseAuthentication();
app.UseAuthorization();
var welcome = app.Configuration["AppSettings:WelcomeMessage"];
var version = app.Configuration["AppSettings:Version"];

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Logger.LogInformation("Застосунок запущено. Середовище: {Env}", app.Environment.EnvironmentName);


app.MapGet("/", () => "MVP Back-end із SQLite працює!")
    .WithTags("Service");

app.MapGet("/medias", async(AppDbContext db) => await db.Medias.ToListAsync())
    .WithTags("Medias");

app.MapGet("/medias/{id}", async(int id, AppDbContext db) =>
await db.Medias.FindAsync(id) is Media media
? Results.Ok(media)
: Results.NotFound())
    .WithTags("Medias");


app.MapPost("/medias", async (Media media, AppDbContext db) =>
{
    db.Medias.Add(media);
    await db.SaveChangesAsync();
    return Results.Created($"/medias/{media.Id}", media);
}).WithTags("Medias");

app.MapPut("/medias/{id}", async (int Id, Media input, AppDbContext db) =>
{
    var media = await db.Medias.FindAsync(Id);
    if (media is null) return Results.NotFound();

    media.Name = input.Name;
    media.pagesQuan = input.pagesQuan;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Medias");


app.MapDelete("/medias/{id}", async (int Id, AppDbContext db) =>
{
    var media = await db.Medias.FindAsync(Id);
    if (media is null) return Results.NotFound();

    db.Medias.Remove(media);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Medias");


app.MapPost("/auth/register", async(RegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
return Results.Conflict("Користувач з таким email вже існує.");

    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}",
new {user.Id, user.Name, user.Email});
}).WithTags("Auth");

app.MapPost("/auth/login", async(LoginDto dto, AppDbContext db,
IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email ==
    dto.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password,
    user.PasswordHash))
        return Results.Unauthorized();

    var token = CreateToken(user, config);
    return Results.Ok(new {access_token = token, token_type = "Bearer"
    });
}).WithTags("Auth");

app.MapGet("/auth/me", (ClaimsPrincipal principal) =>
Results.Ok(new
{

    Id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
    Email = principal.FindFirstValue(ClaimTypes.Email),
}))
.RequireAuthorization()
.WithTags("Auth");


app.Run();



static string CreateToken(User user, IConfiguration config)
{
    var claims = new[]
    {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
};
    var key = new
        SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key,
        SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}
record LoginDto(string Email, string Password);

record RegisterDto(string Name, string Email, string Password);