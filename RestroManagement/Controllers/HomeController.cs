using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;
using RestroManagement.Models;
using System.Diagnostics;

namespace RestroManagement.Controllers
{

    public class HomeController(AppDBContext appDBContext) : Controller
    {
        private readonly AppDBContext _context = appDBContext;

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> RestaurantLandingPage()
        {
            var items = await _context.Fooditems
              .Include(f => f.Portions)
              .Include(f => f.Images)
              .Include(f => f.Categories)
                  .ThenInclude(fc => fc.Category)
              .OrderByDescending(f => f.Created)
              .ToListAsync();
            return View(items);

        }
        public IActionResult CommercialIndex()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
