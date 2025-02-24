using System.Security.Claims;
using CustomUser_Auth.Dtos;
using CustomUser_Auth.Helpers.Services;
using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Cors;
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

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, 
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }
    
    [HttpOptions("loginWithGoogle")]
    public IActionResult Options()
    {
        return Ok();
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }
            return BadRequest(result.Errors);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("createNormalUser")]
    public async Task<IActionResult> CreateNormalUser([FromBody] NormalUser normalUser)
    {
        if (ModelState.IsValid)
        {
            var user = new NormalUser()
            {
                UserName = normalUser.Email,
                Email = normalUser.Email,
                FirstName = normalUser.FirstName,
                LastName = normalUser.LastName,
                PhoneNumber = normalUser.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, normalUser.PasswordHash);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }
            return BadRequest(result.Errors);
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
    
    //With Google Auth 
    [HttpGet("loginWithGoogle")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action(nameof(GoogleResponse), "User", null, Request.Scheme);
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, GoogleDefaults.AuthenticationScheme);
    }
    
    [HttpGet("signin-google")]
    public async Task<IActionResult> GoogleResponse()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return BadRequest("Google Authentication Failed");
        
        var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrEmpty(email))
            return BadRequest("Google response did not include an email");
        
        var existingUser = await _userManager.FindByEmailAsync(email);
        
        if (existingUser == null)
        {
            // New user: Register them
            var user = new NormalUser()
            {
                UserName = email,
                Email = email,
                FirstName = name,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to create user");
        }
        
        return Ok(new { message = "Login successful"});
    }

    [HttpPost("logoutFromGoogleAuth")]
    public async Task<IActionResult> LogoutFromGoogleAuth()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }
}