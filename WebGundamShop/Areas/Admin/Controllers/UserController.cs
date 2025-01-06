using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;
using WebGundamShop.Models;
using WebGundamShop.Repostitory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebGundamShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
	[Authorize(Roles = "Admin")]
	public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;

	    public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
			_dataContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new { User = u, RoleName = r.Name })
                               .ToListAsync();

            return View(usersWithRoles);
        }

        [HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string Id)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(Id);
			if (user == null)
			{
				return NotFound();
			}
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string Id, AppUserModel appUser)
		{
			var currentUser = await _userManager.FindByIdAsync(Id); 
			if (currentUser == null) return NotFound();
			if(ModelState.IsValid)
			{
				currentUser.UserName = appUser.UserName;
				currentUser.Email = appUser.Email;
				currentUser.PhoneNumber = appUser.PhoneNumber;
				currentUser.RoleId = appUser.RoleId;
				var update = await _userManager.UpdateAsync(currentUser);
				if (update.Succeeded)
				{
					TempData["success"] = "Cập nhập người dùng thành công.";
					return RedirectToAction("Index", "User");
				}

				AddIdentityErrors(update);

				return View(currentUser);
			}
			TempData["error"] = "Model validation failed.";
			var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
			string errorMessage = string.Join("\n", errors);

			return View(currentUser);
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		[Route("Create")]
		public async Task<IActionResult> Create(AppUserModel appUser)
		{
			if (ModelState.IsValid)
			{
				var createUser = await _userManager.CreateAsync(appUser, appUser.PasswordHash);
				if (createUser.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(appUser.Email);
					var role = _roleManager.FindByIdAsync(appUser.RoleId);
					var addToRole = await _userManager.AddToRoleAsync(user, role.Result.Name);
					if (!addToRole.Succeeded) AddIdentityErrors(createUser);
					return RedirectToAction("Index", "User");
				}
				else
				{
					AddIdentityErrors(createUser);
					return View(appUser);
				}
			}
            else
            {
				TempData["error"] = "Model có một vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
		}

		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string Id)
		{
			if (string.IsNullOrEmpty(Id)) return NotFound();	

			var user = await _userManager.FindByIdAsync(Id);

			if (user == null) return NotFound();

			var delete = await _userManager.DeleteAsync(user);

			if (!delete.Succeeded) return View("Error");

			TempData["success"] = "User đã xóa thành công!";
			return RedirectToAction("Index");
		}

		private void AddIdentityErrors(IdentityResult identityResult)
		{
			foreach (var error in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
	}
}
