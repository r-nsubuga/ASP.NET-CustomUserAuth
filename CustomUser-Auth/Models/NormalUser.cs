using System.ComponentModel;

namespace CustomUser_Auth.Models;

public class NormalUser: User
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    [DefaultValue(false)]
    public bool IsActive { get; set; }
}