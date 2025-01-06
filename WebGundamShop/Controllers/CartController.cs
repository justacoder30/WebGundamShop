using Microsoft.AspNetCore.Mvc;
using WebGundamShop.Models;
using WebGundamShop.Models.ViewModel;
using WebGundamShop.Repostitory;

namespace WebGundamShop.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index(string slug = "")
		{
			List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemViewModel cartItemViewModel = new()
			{
				CartItems = cartItemModels,
				GrandTotal = cartItemModels.Sum(x => x.Quantity * x.Price)
			};
			return View(cartItemViewModel);
		}

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

		public async Task<IActionResult> Add(long Id)
		{
			ProductModel productModel = await _dataContext.Products.FindAsync(Id);
			List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItemModel = cartItemModels.Where(c => c.ProductId == Id).FirstOrDefault();	

			if (cartItemModel == null)
			{
				cartItemModels.Add(new CartItemModel(productModel));
			}
			else
			{
				cartItemModel.Quantity += 1; 
			}
			HttpContext.Session.SetJson("Cart", cartItemModels);
			TempData["success"] = "Thêm sản phẩm thành công!";
			return Redirect(Request.Headers["Referer"].ToString());
		}

		public async Task<IActionResult> Decrease(long Id)
		{
			List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			CartItemModel cartItemModel = cartItemModels.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItemModel.Quantity > 1)
			{
				--cartItemModel.Quantity;
			}
			else
			{
				cartItemModels.RemoveAll(p => p.ProductId == Id);
			}

			if (cartItemModels.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItemModels);
			}

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Increase(long Id)
		{
			List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			CartItemModel cartItemModel = cartItemModels.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItemModel.Quantity >= 1)
			{
				++cartItemModel.Quantity;
			}
			else
			{
				cartItemModels.RemoveAll(p => p.ProductId == Id);
			}

			if (cartItemModels.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItemModels);
			}

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Remove(long Id)
		{
			List<CartItemModel> cartItemModels = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			CartItemModel cartItemModel = cartItemModels.Where(c => c.ProductId == Id).FirstOrDefault();

			cartItemModels.RemoveAll(p => p.ProductId == Id);


			if (cartItemModels.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItemModels);
			}

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Clear()
		{
			HttpContext.Session.Remove("Cart");
			return RedirectToAction("Index");
		}
	}
}
