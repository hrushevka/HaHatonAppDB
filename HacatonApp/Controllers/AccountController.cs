using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager,
			
			ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_context = context;
		}

		[HttpGet]
		public IActionResult Login() => View();

		[HttpPost]
		public async Task<IActionResult> Register(RegisterNewUserModelQ model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser 
				{
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
				};
			    var result = await _userManager.CreateAsync(user, model.Password);
			}return View();
		}
		public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user != null)
			{
				var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
			}
			ViewBag.Error = "Неверный email или пароль";
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		// Регистрация только для участников (команды)
		[HttpGet]
		public IActionResult RegisterTeam() => View();

		[HttpPost]
		public async Task<IActionResult> RegisterTeam(RegisterTeamViewModel model)
		{
			if (ModelState.IsValid)
			{	
				var capitain = await _userManager.FindByIdAsync(model.CapitainId);
				if (capitain == null) return NotFound();
				var team = new Team
				{
					Name = model.Name,
					ContactEmail = capitain.Email
				};
				var teamObj = _context.Teams.Add(team);

				var curTeamId = teamObj.Entity.Id;



                var usersIdArray = model.UsersId.ToArray();

				for (int i = 0; i< usersIdArray.Length; i++)
				{
					var user = await _userManager.FindByIdAsync(usersIdArray[i]);
					if (user == null) continue;
					user.TeamID = curTeamId;
					var resultUpd = await _userManager.UpdateAsync(user);
					if (resultUpd.Succeeded)
					{
						await _userManager.AddToRoleAsync(user, "");
					}
					foreach (var error in resultUpd.Errors)
						ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}
	}
}
