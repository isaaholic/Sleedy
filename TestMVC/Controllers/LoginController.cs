using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using TestMVC.Models;
using TestMVC.Repo;

namespace TestMVC.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Login");
        }
        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminDto admin)
        {
            HttpClient client = new HttpClient();
            //admin = new()
            //{
            //    UserName = "admin",
            //    Password = "admin",
            //    FirstName = "s",
            //    LastName = "a",
            //};
            client.BaseAddress = new Uri("http://localhost:5074/");
            var response = await client.PostAsJsonAsync("api/Admins/login", admin);
            if (response.IsSuccessStatusCode)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,$"{admin.UserName}"),
                new Claim(ClaimTypes.Role,"Admin")
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);
                LocalDb.Request = admin;
                return RedirectToAction("Index", "Home");
            }

            ViewData["ValidateMessage"] = "user not found";
            return View();  
        }
    }
}
