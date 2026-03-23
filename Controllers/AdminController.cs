using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Data;
using WebPartDashboard.Models;
using WebPartDashboard.Services;
using System.ComponentModel.DataAnnotations;

namespace WebPartDashboard.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public AdminController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == viewModel.UserName))
            {
                ModelState.AddModelError("UserName", "Пользователь с таким именем уже существует");
                return View(viewModel);
            }

            var (hash, salt) = _authService.HashPassword(viewModel.Password);
            
            var user = new User
            {
                UserName = viewModel.UserName,
                PasswordHash = hash,
                Salt = salt,
                Role = string.IsNullOrEmpty(viewModel.Role) ? "User" : viewModel.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Role = user.Role
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.UserName = viewModel.UserName;
                existingUser.Role = viewModel.Role;
                
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(viewModel.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    public string Role { get; set; } = "User";
}

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    public string Role { get; set; } = "User";
}
