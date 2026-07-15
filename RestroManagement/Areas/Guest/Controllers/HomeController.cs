using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;
using RestroManagement.DbModels;
using RestroManagement.DbModels.User;
using RestroManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RestroManagement.Areas.Guest.Controllers
{
    [Area("Guest")]
    //[Authorize(Roles = "Guest")]
    public class HomeController : Controller
    {
        private readonly AppDBContext dBContext;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            AppDBContext _dBContext,
            UserManager<AppUser> userManager)
        {
            dBContext = _dBContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Profile1()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "Guest";

            var orders = dBContext.Orders
                .Where(x => x.UserId == user.Id.ToString())
                      .ToList();

            ViewBag.Orders = orders ?? new List<Order>();

            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            ViewBag.Role = roles.FirstOrDefault() ?? "Guest";

            var model = new profile
            {
                Id = currentUser.Id,
                FName = currentUser.FName,
                LName = currentUser.LName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber ?? string.Empty,
                Address = currentUser.Address ?? string.Empty,
                ProfileImage = currentUser.ProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(profile model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                currentUser.FName = model.FName;
                currentUser.LName = model.LName;
                currentUser.PhoneNumber = model.PhoneNumber;
                currentUser.Address = model.Address;

                if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile_pics");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfileImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImageFile.CopyToAsync(fileStream);
                    }

                    // Delete old profile picture if exists and is not default
                    if (!string.IsNullOrEmpty(currentUser.ProfilePicture))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", currentUser.ProfilePicture.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try { System.IO.File.Delete(oldFilePath); } catch { /* ignore if locked */ }
                        }
                    }

                    currentUser.ProfilePicture = "/uploads/profile_pics/" + uniqueFileName;
                }

                var result = await _userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile1");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            ViewBag.Role = roles.FirstOrDefault() ?? "Guest";
            return View(model);
        }

        public async Task<IActionResult> Menu()
        {
            var items = await dBContext.Fooditems
                .Include(f => f.Categories)
                    .ThenInclude(fc => fc.Category)
                .Include(f => f.Images)
                .Include(f => f.Portions)
                .ToListAsync();

            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            ViewBag.CartCount = cart.Sum(x => x.Quantity);

            return View(items);
        }

        [HttpPost]
        public IActionResult AddMultipleToCart(
            List<int> FoodItemId, 
            List<string> ItemName, 
            List<string> PortionName, 
            List<double> Price, 
            List<int> Quantity)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            if (FoodItemId != null)
            {
                for (int i = 0; i < FoodItemId.Count; i++)
                {
                    if (Quantity[i] > 0)
                    {
                        var existingItem = cart.FirstOrDefault(item => 
                            item.FoodItemId == FoodItemId[i] && 
                            item.PortionName == PortionName[i]);

                        if (existingItem != null)
                        {
                            existingItem.Quantity += Quantity[i];
                            existingItem.Price = Price[i]; // Update price to latest selected
                        }
                        else
                        {
                            cart.Add(new CartItem
                            {
                                FoodItemId = FoodItemId[i],
                                ItemName = ItemName[i],
                                PortionName = PortionName[i],
                                Price = Price[i],
                                Quantity = Quantity[i]
                            });
                        }
                    }
                }
            }

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateCart(
            List<int> FoodItemId, 
            List<string> ItemName, 
            List<string> PortionName, 
            List<double> Price, 
            List<int> Quantity, 
            string submitAction)
        {
            var cart = new List<CartItem>();

            if (FoodItemId != null)
            {
                for (int i = 0; i < FoodItemId.Count; i++)
                {
                    if (Quantity[i] > 0)
                    {
                        cart.Add(new CartItem
                        {
                            FoodItemId = FoodItemId[i],
                            ItemName = ItemName[i],
                            PortionName = PortionName[i],
                            Price = Price[i],
                            Quantity = Quantity[i]
                        });
                    }
                }
            }

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            if (submitAction == "checkout")
            {
                return RedirectToAction("Checkout", "Order");
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int FoodItemId, string PortionName)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                var itemToRemove = cart.FirstOrDefault(i => i.FoodItemId == FoodItemId && i.PortionName == PortionName);
                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                    HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                }
            }
            return RedirectToAction("Cart");
        }

        public IActionResult Cart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // Populate portion options for editing in cart
            foreach (var item in cart)
            {
                var foodItem = dBContext.Fooditems
                    .Include(f => f.Portions)
                    .FirstOrDefault(f => f.Id == item.FoodItemId);

                if (foodItem?.Portions != null)
                {
                    item.Portions = foodItem.Portions.Select(p => new PortionOption
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToList();
                }
            }

            return View(cart);
        }
    }
}
