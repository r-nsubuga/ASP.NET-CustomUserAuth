using CustomUser_Auth.Data;
using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddDbContext<UserDbContext>(options =>
//     options.UseInMemoryDatabase("AuthDb"));

var connectionString = builder.Configuration.GetConnectionString("userString");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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

// builder.Services.AddIdentity<User, IdentityRole>(options =>
// {
//     options.User.RequireUniqueEmail = true;
//     options.Lockout.MaxFailedAccessAttempts = 5;
//     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//     options.Lockout.AllowedForNewUsers = true;
//     options.SignIn.RequireConfirmedEmail = true;
// });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();

var app = builder.Build();

app.MapControllers();
// app.MapIdentityApi<User>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

