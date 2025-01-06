using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebGundamShop.Models;
using WebGundamShop.Repostitory;

namespace WebGundamShop.Controllers
{
    public class HomeController : Controller
    {
		private readonly DataContext _dataContext;
		private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
			_dataContext = context;

		}

        public async Task<IActionResult> Index(int pg = 1, string sort_by = "")
        {
            List<ProductModel> products = await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync();
			var count = products.Count();

			if (count > 0)
			{
				if (sort_by == "price_increase")
				{
					products = await _dataContext.Products.OrderBy(p => p.Price).Include(p => p.Category).Include(p => p.Brand).ToListAsync();
				}
				else if (sort_by == "price_decrease")
				{
					products = await _dataContext.Products.OrderByDescending(p => p.Price).Include(p => p.Category).Include(p => p.Brand).ToListAsync();
				}
			}

			const int pageSize = 9;

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound404");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
