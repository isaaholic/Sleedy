using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Web.Models;
using WebAPI.Models;

namespace Web.Pages
{
    [Authorize]
    [BindProperties(SupportsGet =true)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _env;
        public FacultiesResponse facResp { get; set; }
        public string url { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
            facResp = new();
            HttpClient client = new();
            client.BaseAddress = new Uri("https://sleedy.azurewebsites.net/");
            facResp = client.GetFromJsonAsync<FacultiesResponse>("api/Admins/response?facName=ITIF").Result;
            if(facResp!=null)
            url = facResp.ImageUrl;
        }

        public void OnGet()
        {

        }

        public async void OnPost(IFormFile file)
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
                    client.BaseAddress = new Uri("https://sleedy.azurewebsites.net/");
                    FacultiesResponse resp = new();
                    resp.FacultyName = "ITIF";
                    resp.ImageUrl = link;
                    var response = await client.PutAsJsonAsync("api/Admins/response", resp);
                    if (response.IsSuccessStatusCode)
                        return;
                    throw new Exception();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    stream.Close();
                }

            }
            return;
        }
    }
}