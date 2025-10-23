using DeviceManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
// using Microsoft.IdentityModel.Logging;
// IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Database
var conStrBuilder = new MySqlConnector.MySqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")!)
{
    Password = builder.Configuration["DATABASE_PASS"],
    UserID = builder.Configuration["DATABASE_USER"]
};

builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseMySql(conStrBuilder.ConnectionString, ServerVersion.AutoDetect(conStrBuilder.ConnectionString))
);

// Authentication (JWT) - .NET 7 style
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Force old JwtSecurityToken handler (like .NET 7)
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"Token validation failed: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var jwt = ctx.SecurityToken as JwtSecurityToken;
                if (jwt != null)
                {
                    Console.WriteLine($"Token validated successfully. JTI: {jwt.Id}, Sub: {jwt.Subject}");
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
