using Microsoft.AspNetCore.Mvc;
using System;
using WebAPI.Models;

namespace TestMVC.Controllers
{
    public class ITIFController : Controller
    {
        public FacultiesResponse facResp { get; set; }
        public string url { get; set; }
        public string movurl { get; set; }
        public ITIFController()
        {
            facResp = new();
            HttpClient client = new();
            client.BaseAddress = new Uri("http://localhost:5074/");
            facResp = client.GetFromJsonAsync<FacultiesResponse>("api/Admins/response?facName=ITIF").Result;
            if (facResp != null)
            {
                if (facResp.ImageUrl.Contains(".mp4"))
                    movurl = facResp.ImageUrl;
                else
                    url = facResp.ImageUrl;
            }
        }

        public IActionResult Index()
        {
            if (url != null)
                ViewBag.image = url.ToString();
            else
                ViewBag.vid = movurl.ToString();

            ViewBag.author = facResp.Author;
            return View();
        }
    }
}
