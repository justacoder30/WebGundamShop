using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebGundamShop.Models.ViewModel;
using WebGundamShop.Models;
using WebGundamShop.Repostitory;
using System.Security.Claims;
using WebGundamShop.Areas.Admin.Repostitory;

namespace WebGundamShop.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public CheckoutController(DataContext dataContext, IEmailSender emailSender)
		{
			_dataContext = dataContext;
			_emailSender = emailSender;
		}
		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var orderCode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = orderCode;
				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;
				_dataContext.Add(orderItem);	
				_dataContext.SaveChanges();
				List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach (var cartItemModel in cartItemModels)
				{
					var oderdetail = new OrderDetail();
					oderdetail.UserName = userEmail;
					oderdetail.OrderCode = orderCode;
					oderdetail.ProductId = cartItemModel.ProductId;
					oderdetail.Price = cartItemModel.Price;
					oderdetail.Quantity = cartItemModel.Quantity;
					_dataContext.Add(oderdetail);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("Cart");
				TempData["success"] = "Checkout thành công, xin hãy chờ duyệt đơn hàng";
				return RedirectToAction("Index", "Cart");
			}
			return View();
		}
	}
}
