namespace CustomUser_Auth.Models;

public class BusinessUser: User
{
    public string? BusinessName { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public string? Location { get; set; }
    public string? ContactNumber { get; set; }
}