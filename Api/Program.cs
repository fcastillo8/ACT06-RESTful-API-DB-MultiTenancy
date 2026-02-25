using System.Text;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Logging;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERILOG CONFIGURATION
// ==========================================
SerilogConfig.ConfigureSerilog(builder.Host);

// ==========================================
// 2. DATABASE CONFIGURATION (SQL Server)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings_DefaultConnection"]
    ?? "Server=localhost;Database=MultiTenantDb;Trusted_Connection=true;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================
// 3. JWT AUTHENTICATION CONFIGURATION
// ==========================================
var jwtKey = builder.Configuration["JWT_Key"]
    ?? builder.Configuration["Jwt:Key"]
    ?? "SuperSecretKey12345678901234567890!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MultiTenantApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MultiTenantApiClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// ==========================================
// 4. DEPENDENCY INJECTION
// ==========================================
builder.Services.AddHttpContextAccessor();

// Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

// ==========================================
// 5. CONTROLLERS & SWAGGER
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MultiTenant API",
        Version = "v1",
        Description = "API RESTful con autenticación JWT y multitenancy. ACT06 - Actividad Grupal.",
        Contact = new OpenApiContact
        {
            Name = "Equipo ACT06"
        }
    });

    // Configure Swagger to accept JWT Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer.\n\n" +
                      "Ingrese 'Bearer' [espacio] y luego su token.\n\n" +
                      "Ejemplo: \"Bearer eyJhbGciOiJIUzI1NiIs...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// 6. CORS
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ==========================================
// 7. MIDDLEWARE PIPELINE
// ==========================================

// Serilog request logging
app.UseSerilogRequestLogging();

// Swagger (always enabled for Railway)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiTenant API v1");
    c.RoutePrefix = string.Empty; // Swagger at root URL
});

app.UseCors("AllowAll");

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================================
// 8. DATABASE MIGRATION (Auto-migrate on startup)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Log.Information("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database migration failed. Ensuring database is created...");
        dbContext.Database.EnsureCreated();
        Log.Information("Database created successfully.");
    }
}

// ==========================================
// 9. START APPLICATION
// ==========================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

Log.Information("MultiTenant API starting on port {Port}...", port);
app.Run();
