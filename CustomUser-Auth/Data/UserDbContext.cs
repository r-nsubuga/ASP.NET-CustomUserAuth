using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomUser_Auth.Data;

public class UserDbContext: IdentityDbContext<User>
{
    public UserDbContext(DbContextOptions<UserDbContext> options): base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}