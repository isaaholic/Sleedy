using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TestMVC.Models;
using TestMVC.Repo;
using WebAPI.Models;

namespace TestMVC.Controllers
{
    [BindProperties(SupportsGet = true)]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public Admin User { get; set; }
        public FacultiesResponse facResp { get; set; }
        public string url { get; set; }
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
            facResp = new();
            HttpClient client = new();
            client.BaseAddress = new Uri("http://localhost:5074/");
            facResp = client.GetFromJsonAsync<FacultiesResponse>("api/Admins/response?facName=ITIF").Result;
            if (facResp != null)
                url = facResp.ImageUrl;
        }

        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            var ex = claimUser.Claims.ElementAtOrDefault(0);
            HttpClient client = new();

            client.BaseAddress = new Uri("http://localhost:5074/");
            var admin = client.GetFromJsonAsync<Admin>($"api/Admins/{ex.Value}").Result;
            if(admin!=null)
            {
                LocalDb.Admin = admin;
                ViewBag.admin = admin;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var fileupload = file;
            FileStream stream;
            if (fileupload.Length > 0)
            {
                string foldername = "fireBaseFiles";
                if (string.IsNullOrWhiteSpace(_env.WebRootPath))
                {
                    _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                string path = Path.Combine(_env.WebRootPath, $"images/{foldername}");
                if (Directory.Exists(path))
                {
                    using (stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Create))
                    {
                        try
                        {
                            await fileupload.CopyToAsync(stream);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    stream.Close();
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Open);
                }
                else
                {
                    Directory.CreateDirectory(path);
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Create);
                    try
                    {
                        await fileupload.CopyToAsync(stream);
                    }
                    catch (Exception)
                    {

                    }
                    stream.Close();
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Open);
                }



                var auth = new FirebaseAuthProvider(new FirebaseConfig(FireBase.ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(FireBase.AuthEmail, FireBase.AuthPassword);
                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    FireBase.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("images")
                    .Child(DateTime.Now.ToString())
                    .Child($"{fileupload.FileName}.{Path.GetExtension(fileupload.FileName).Substring(1)}")
                    .PutAsync(stream, cancellation.Token);

                try
                {
                    string link = await task;
                    stream.Close();
                    Directory.Delete(path, true);
                    HttpClient client = new();
                    client.BaseAddress = new Uri("http://localhost:5074/");
                    FacultiesResponse resp = new();
                    resp.FacultyName = "ITIF";
                    resp.Author = LocalDb.Admin.FirstName + " " + LocalDb.Admin.LastName;
                    resp.ImageUrl = link;
                    var response = await client.PutAsJsonAsync("api/Admins/response", resp);
                    if (response.IsSuccessStatusCode)
                        return this.Index();

                    throw new Exception();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    stream.Close();
                }

            }
            return this.Index(); 
        }
    }
}