using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebGundamShop.Models;
using WebGundamShop.Repostitory;

namespace WebGundamShop.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index(string Slug = "", int pg = 1, string sort_by = "")
		{
			BrandModel brandModel = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
			if (brandModel == null) return RedirectToAction("Index");
			IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandId == brandModel.Id);
			var count = await productsByBrand.CountAsync();

			if (count > 0)
			{
				if (sort_by == "price_increase")
				{
					productsByBrand = productsByBrand.OrderBy(p => p.Price);
				}
				else if (sort_by == "price_decrease")
				{
					productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
				}
			}

			var products = await productsByBrand.ToListAsync();

			const int pageSize = 6;

			if (pg < 1)
			{
				pg = 1;
			}
			int recsCount = products.Count();

			var pager = new Paginate(recsCount, pg, pageSize);

			int recSkip = (pg - 1) * pageSize;

			var data = products.Skip(recSkip).Take(pager.PageSize).ToList();

			ViewBag.Pager = pager;

			return View(data);
		}

	}
}
