using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using AssetManagement.Configurations;
using AssetManagement.Repository;



var builder = WebApplication.CreateBuilder(args);

// Load Stripe Secret Key from appsettings.json
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrEmpty(stripeSecretKey))
{
    throw new ArgumentNullException("Stripe:SecretKey is missing in appsettings.json");
}
StripeConfiguration.ApiKey = stripeSecretKey;

// Database Context
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPhysicalAssetRepository, PhysicalAssetRepository>();
builder.Services.AddScoped<ISoftwareAssetRepository, SoftwareAssetRepository>();
builder.Services.AddScoped<IEmployeePhysicalAssetRepository, EmployeePhysicalAssetRepository>();
builder.Services.AddScoped<IEmployeeSoftwareAssetRepository, EmployeeSoftwareAssetRepository>();
builder.Services.AddScoped<IRoleMasterRepository, RoleMasterRepository>();
builder.Services.AddScoped<IAssetRequestRepository, AssetRequestRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IEmployeeAssetTransactionRepository, EmployeeAssetTransactionRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));


// Enable CORS for React Frontend 
// ✅ CORS Policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                              "https://asset-management-tailor-management.vercel.app",
                              "https://asset-management-git-main-tailor-management.vercel.app"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

// Configure Authentication with JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is missing");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdmin", policy => policy.RequireRole("1"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("2"));
    options.AddPolicy("RequireEmployee", policy => policy.RequireRole("3"));
});

// Add Controllers
builder.Services.AddControllers();


// Configure Swagger for JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] {}
        }
    });
});
 var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Management API v1");
    });
}

app.UseCors(MyAllowSpecificOrigins);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "🚀 Asset API is running!"); // ✅ Add this
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080"; // ✅ Add this
app.Urls.Add($"http://*:{port}");

app.Run();
