using Microsoft.AspNetCore.Mvc;

namespace WebGundamShop.Controllers
{
	public class LoginController: Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
