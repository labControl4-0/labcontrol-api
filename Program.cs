using Microsoft.EntityFrameworkCore;
using LabControlApi.Data;
using LabControlApi.Repositories;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services;
using LabControlApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Configurar CORS para liberar o frontend Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000", "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});


// Configurar DbContext para PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=postgres"
    ));

// Registrar repositórios e serviços
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IPlantVersionRepository, PlantVersionRepository>();
builder.Services.AddScoped<IPlantVersionService, PlantVersionService>();
builder.Services.AddScoped<ISectorRepository, SectorRepository>();
builder.Services.AddScoped<ISectorService, SectorService>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IMachineMetricRepository, MachineMetricRepository>();
builder.Services.AddScoped<IMachineMetricService, MachineMetricService>();
builder.Services.AddScoped<IMachineEventRepository, MachineEventRepository>();
builder.Services.AddScoped<IMachineEventService, MachineEventService>();

// Adicionar controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT key is not configured.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar CORS antes de autenticação
app.UseCors("AllowFrontend");


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
