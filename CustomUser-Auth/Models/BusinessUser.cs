using System.ComponentModel;
using CustomUser_Auth.Enums;

namespace CustomUser_Auth.Models;

public class BusinessUser: User
{
    public string? BusinessName { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public string? Location { get; set; }
    public string? ContactNumber { get; set; }
    public VendorType? VendorType { get; set; }
    [DefaultValue(false)]
    public bool IsVerified { get; set; }
    [DefaultValue(false)]
    public bool IsActive { get; set; }
}