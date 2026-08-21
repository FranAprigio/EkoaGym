using System.Text;
using EkoaGym.Api.ExceptionHandling;
using EkoaGym.Application.Auth;
using EkoaGym.Application.BodyMetrics;
using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Exercises;
using EkoaGym.Application.Goals;
using EkoaGym.Application.Plans;
using EkoaGym.Application.Profiles;
using EkoaGym.Application.Progress;
using EkoaGym.Application.ProgressPhotos;
using EkoaGym.Application.Sessions;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.BodyMetrics;
using EkoaGym.Infrastructure.Exercises;
using EkoaGym.Infrastructure.Goals;
using EkoaGym.Infrastructure.Persistence;
using EkoaGym.Infrastructure.Plans;
using EkoaGym.Infrastructure.Profiles;
using EkoaGym.Infrastructure.Progress;
using EkoaGym.Infrastructure.ProgressPhotos;
using EkoaGym.Infrastructure.Security;
using EkoaGym.Infrastructure.Sessions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string MobileAppCorsPolicy = "MobileApp";

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Paste the access token (without the 'Bearer ' prefix)."
    });
    options.AddSecurityRequirement(_ => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<EkoaGymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ProgressionOptions>(builder.Configuration.GetSection(ProgressionOptions.SectionName));
builder.Services.Configure<FileStorageOptions>(options =>
{
    options.UploadsRootPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
});

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<IProgressionService, ProgressionService>();
builder.Services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();
builder.Services.AddScoped<IBodyMetricService, BodyMetricService>();
builder.Services.AddScoped<IProgressPhotoService, ProgressPhotoService>();
builder.Services.AddScoped<IExerciseProgressService, ExerciseProgressService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Read the raw "sub" claim instead of the legacy ClaimTypes.NameIdentifier remapping.
        options.MapInboundClaims = false;
    });

// Configured via IOptions (resolved lazily at first use), NOT a value captured before Build() — a
// pre-Build() snapshot of configuration misses overrides applied by things like WebApplicationFactory
// in integration tests, which only take effect during Build().
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(MobileAppCorsPolicy, policy =>
    {
        // Dev-only fallback so the mobile app can reach the API before origins are configured.
        if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Ensure the mounted uploads volume/bind mount has a directory to attach to.
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "uploads"));

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MobileAppCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can be used from integration tests.
public partial class Program;
