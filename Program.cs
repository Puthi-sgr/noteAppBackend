using System.Data;
using Microsoft.Data.SqlClient;
using NoteApp.Repositories;
using NoteApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register database connection (Scoped per request)
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    return new SqlConnection(cs);
});

// Repositories dependency injection here
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

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
