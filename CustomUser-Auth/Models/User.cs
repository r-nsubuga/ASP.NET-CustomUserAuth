using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomUser_Auth.Models;

public class User : IdentityUser
{
    public string firstName { get; set; }
    public string lastName { get; set; }
}