using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Services;
using WebPartDashboard.Data;
using WebPartDashboard.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace WebPartDashboard.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);

        if (user == null || !_authService.VerifyPassword(model.Password, user.PasswordHash, user.Salt))
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль");
            return View(model);
        }

        var token = _authService.GenerateJwtToken(user);
        HttpContext.Session.SetString("JWToken", token);
        
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
        {
            ModelState.AddModelError("", "Пользователь с таким именем уже существует");
            return View(model);
        }

        var (hash, salt) = _authService.HashPassword(model.Password);
        var user = new User
        {
            UserName = model.UserName,
            PasswordHash = hash,
            Salt = salt,
            Role = string.IsNullOrEmpty(model.Role) ? "User" : model.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);
        HttpContext.Session.SetString("JWToken", token);
        
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWToken");
        return RedirectToAction("Login");
    }
}

public class LoginModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    public string Password { get; set; } = string.Empty;
    
    public string Role { get; set; } = "User";
}
