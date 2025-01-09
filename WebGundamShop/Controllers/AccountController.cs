using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebGundamShop.Areas.Admin.Repostitory;
using WebGundamShop.Models;
using WebGundamShop.Models.ViewModel;
using WebGundamShop.Repostitory;

namespace WebGundamShop.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IEmailSender _emailSender;
		private readonly DataContext _dataContext;
		public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager, DataContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_dataContext = context;
			_roleManager = roleManager;
		}

		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel
			{
				ReturnUrl = returnUrl,
			});
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginViewModel)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginViewModel.Username, loginViewModel.Password, false, false);
				if (result.Succeeded)
				{
					return Redirect(loginViewModel.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Username hoặc Password không đúng.");
			}
			return View(loginViewModel);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(UserModel userModel)
		{
			if (ModelState.IsValid)
			{
				AppUserModel newAppUser = new AppUserModel
				{
					UserName = userModel.Username,
					Email = userModel.Email,
					RoleId = "1",
				};
				
				IdentityResult result = await _userManager.CreateAsync(newAppUser, userModel.Password);

				if (result.Succeeded)
				{
					TempData["success"] = "Tạo user thành công!";
					var user = await _userManager.FindByEmailAsync(newAppUser.Email);
					var role = _roleManager.FindByIdAsync(newAppUser.RoleId);
					var addToRole = await _userManager.AddToRoleAsync(user, role.Result.Name);
					return Redirect("/account/login");
				} 
				foreach(IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View();
		}

		
		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
			return Redirect(returnUrl);
		}

        public IActionResult ForgotPass()
        {
            return View();
        }

		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Email not found or token is not right";
				return RedirectToAction("ForgotPass", "Account");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

			if (checkMail == null)
			{
				TempData["error"] = "Email not found";
				return RedirectToAction("ForgotPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();
				var receiver = checkMail.Email;
				var subject = "Change password for user " + checkMail.Email;
				var message = "Click on link to change password " +
					"<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "'>";

				await _emailSender.SendEmailAsync(receiver, subject, message);
			}


			TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
			return RedirectToAction("ForgotPass", "Account");
		}

		public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				//update user with new password and token
				string newtoken = Guid.NewGuid().ToString();
				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

				checkuser.PasswordHash = passwordHash;
				checkuser.Token = newtoken;

				await _userManager.UpdateAsync(checkuser);
				TempData["success"] = "Password updated successfully.";
				return RedirectToAction("Login", "Account");
			}

			TempData["error"] = "Email not found or token is not right";
			return RedirectToAction("ForgotPass", "Account");
		}

		public async Task<IActionResult> History()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			var Orders = await _dataContext.OrderModel
				.Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
			ViewBag.UserEmail = userEmail;
			return View(Orders);
		}

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.OrderModel.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return BadRequest("An error occurred while canceling the order.");
            }


            return RedirectToAction("History", "Account");
        }

		/*public async Task<IActionResult> UpdateAccount()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
			{
				return NotFound();
			}

			return View();
		}*/
	}
}
