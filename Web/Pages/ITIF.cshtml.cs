using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAPI.Models;

namespace Web.Pages
{
    public class ITIFModel : PageModel
    {
        public FacultiesResponse facResp { get; set; }
        public string url { get; set; }

        public ITIFModel()
        {
            facResp = new();
            HttpClient client = new();
            client.BaseAddress = new Uri("https://sleedy.azurewebsites.net/");
            facResp = client.GetFromJsonAsync<FacultiesResponse>("api/Admins/response?facName=ITIF").Result;
            if (facResp != null)
                url = facResp.ImageUrl;
        }
        public void OnGet()
        {
        }
    }
}
