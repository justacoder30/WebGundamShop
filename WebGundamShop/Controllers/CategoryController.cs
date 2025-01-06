using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGundamShop.Models;
using WebGundamShop.Repostitory;

namespace WebGundamShop.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index(string slug = "", int pg = 1, string sort_by = "")
		{
			CategoryModel categoryModel = _dataContext.Categories.Where(c => c.Slug == slug).FirstOrDefault();
			if (categoryModel == null) return RedirectToAction("Index");
			IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == categoryModel.Id);
			var count = await productsByCategory.CountAsync();

			if (count > 0)
			{
				if (sort_by == "price_increase")
				{
					productsByCategory = productsByCategory.OrderBy(p => p.Price);
				}
				else if (sort_by == "price_decrease")
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
				}
			}

			var products = await productsByCategory.ToListAsync();

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
