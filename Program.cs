using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoteApp.Repositories;
using NoteApp.Services;
using System;
using System.Data;
using System.Text;
using static System.Net.WebRequestMethods;
using NoteApp.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigins = new[]
{
    "http://localhost:5173",                             // Vite dev
    "https://github.com/Puthi-sr/noteapp",             // GH Pages (prod) - replace with actual frontend origin if different
    "https://Puthi-sgr.github.io",
    "https://puthinote-proxy.chethaputhi.workers.dev"    // Cloudflare Worker proxy
};

builder.Services.AddCors(o => 
    o.AddPolicy("frontend", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()         // includes Authorization, Content-Type
         .AllowAnyMethod()         // GET/POST/PUT/DELETE/OPTIONS
));

// Register database connection (Scoped per request)
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    return new SqlConnection(cs);
});

// Repositories dependency injection here
var efConn = builder.Configuration.GetConnectionString("EfLearning")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:EfLearning");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(efConn));
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Services dependency injection here
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<NoteApp.Domain.User>, Microsoft.AspNetCore.Identity.PasswordHasher<NoteApp.Domain.User>>();

// JWT Authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
