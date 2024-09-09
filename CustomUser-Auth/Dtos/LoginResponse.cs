using CustomUser_Auth.Models;

namespace CustomUser_Auth.Dtos;

public class LoginResponse
{
    public string Token { get; set; }
    public User? User { get; set; }
}