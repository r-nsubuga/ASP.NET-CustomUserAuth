using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomUser_Auth.Data;

public class UserDbContext: IdentityDbContext<User>
{
    public DbSet<NormalUser> NormalUsers { get; set; }
    public DbSet<BusinessUser> BusinessUsers { get; set; }
    public UserDbContext(DbContextOptions<UserDbContext> options): base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        //to use TPT(Table Per Type)
        builder.Entity<NormalUser>().ToTable("NormalUsers");
        builder.Entity<BusinessUser>().ToTable("BusinessUsers");
    }
}