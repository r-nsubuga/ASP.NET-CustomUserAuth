using CustomUser_Auth.Dtos;
using CustomUser_Auth.Helpers.Services;
using CustomUser_Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomUser_Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
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
                Token = token,
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
}