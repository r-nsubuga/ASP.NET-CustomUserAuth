using System.Text;
using CustomUser_Auth.Data;
using CustomUser_Auth.Helpers.ExceptionHandler;
using CustomUser_Auth.Helpers.Services;
using CustomUser_Auth.Models;
using dotenv.net;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(new DotEnvOptions(envFilePaths: new []{dotenv}));

var logger = Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddDbContext<UserDbContext>(options =>
//     options.UseInMemoryDatabase("AuthDb"));

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
    {
        p.WithOrigins("http://localhost:5173")
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
            ;
    });
});

var connectionString = builder.Configuration.GetConnectionString("userString");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    }).AddCookie(op =>
    {
        op.Cookie.Name = "customAuthCookie";
        op.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        op.Cookie.SameSite = SameSiteMode.None; 
        op.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    })
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("APP_CLIENT_ID");
        options.ClientSecret = Environment.GetEnvironmentVariable("APP_CLIENT_SECRET");
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<UserDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedEmail = false;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddRazorPages();

var app = builder.Build();

app.UseExceptionHandler("/error"); 

// app.Use(async (context, next) =>
// {
//     logger.Information("Before executing middleware: {Path}", context.Request.Path);
//
//     await next();
//
//     logger.Information("After executing middleware: {Path}", context.Request.Path);
// });

app.UseRouting();
app.UseCors();
//app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

