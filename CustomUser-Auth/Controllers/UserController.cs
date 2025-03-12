using System.Net;
using System.Security.Claims;
using CustomUser_Auth.Dtos;
using CustomUser_Auth.Helpers.Emailing;
using CustomUser_Auth.Helpers.Services;
using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomUser_Auth.Controllers;

[ApiController]
//[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly TokenService _tokenService;
    private readonly GoogleAuthService _googleAuthService;
    private readonly IEmailService _emailService;

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, 
        TokenService tokenService, GoogleAuthService googleAuthService, IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _googleAuthService = googleAuthService;
        _emailService = emailService;
    }

    [HttpPost("createNormalUser")]
    public async Task<IActionResult> CreateNormalUser([FromBody] RegisterDto normalUser)
    {
        if (ModelState.IsValid)
        {
            var user = new NormalUser()
            {
                UserName = normalUser.Email,
                Email = normalUser.Email,
                FirstName = normalUser.FirstName,
                LastName = normalUser.LastName,
            };
            var result = await _userManager.CreateAsync(user, normalUser.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            
            var confirmationLink = Url.Action("ConfirmEmail", "User", new { userId = user.Id, token = token },Request.Scheme);
            await _emailService.SendEmailAsync(user.Email, "Email Verification", confirmationLink);
            // await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok();
        }
        return BadRequest(ModelState);
    }
    
    [HttpPost("createBusinessUser")]
    public async Task<IActionResult> CreateBusinessUser([FromBody] BusinessUser businessUser)
    {
        if (ModelState.IsValid)
        {
            var user = new BusinessUser()
            {
                UserName = businessUser.Email,
                Email = businessUser.Email,
                BusinessName = businessUser.BusinessName,
                BusinessRegistrationNumber = businessUser.BusinessRegistrationNumber,
                Location = businessUser.Location,
                PhoneNumber = businessUser.ContactNumber,
            };
            var result = await _userManager.CreateAsync(user, businessUser.PasswordHash);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }
            return BadRequest(result.Errors);
        }
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email,string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { message = "Account doesn't exist." });
        }
        
        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
        Console.WriteLine(result.Succeeded);
        if (result.Succeeded)
        {
            var token = _tokenService.GenerateJwtToken(user);
            var response = new LoginResponse
            {
                AccessToken = token,
                User = user,
            };
            return Ok(response);
        }

        if (result.IsLockedOut)
        {
            return Problem("The account is locked out");
        }

        await _userManager.AccessFailedAsync(user);

        return NotFound(new { message = "Wrong credentials" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(true);
    }
    
    [HttpPost("signin-google")]
    public async Task<IActionResult> GoogleAuth([FromBody] AuthTokenResponse tokenResponse)
    {
        try
        {
            // Validate the Google ID token received from the frontend
            var payload = await _googleAuthService.VerifyGoogleTokenAsync(tokenResponse.Token);

            // You can now use the payload to retrieve user info
            // Example: payload.Email, payload.Name, etc.

            // Return a JWT or some other custom token for your application
            return Ok(new { Message = "Token is valid", User = payload.Email });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
    
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return Problem("Token and user id are required.");
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return BadRequest("User not found");
        }
        var decodedToken = WebUtility.UrlDecode(token);
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        Console.WriteLine(result);
        if (result.Succeeded)
        {
            // Email confirmed successfully
            return Ok();
        }

        // Error during email confirmation
        return Problem("Error");
    }
    [HttpPost("resend-email")]
    public async Task<IActionResult> ResendConfirmationEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not { EmailConfirmed: false }) return Ok("Email already confirmed.");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

        await _emailService.SendEmailAsync(user.Email, "Email Verification", confirmationLink);
        return Ok();

    }
}