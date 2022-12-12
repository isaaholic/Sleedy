using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Controllers.Dtos;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly IConfiguration _config;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public AdminsController(ServerContext context, IConfiguration config, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;
        }

        // GET: api/Admins
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdmins()
        {
            if (_context.Admins == null)
            {
                return NotFound();
            }
            return await _context.Admins.ToListAsync();
        }

        // GET: api/Admins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(string id)
        {
            if (_context.Admins == null)
            {
                return NotFound();
            }
            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return admin;
        }

        // PUT: api/Admins/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(string id, Admin admin)
        {
            if (id != admin.UserName)
            {
                return BadRequest();
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Admins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            if (_context.Admins == null)
            {
                return NotFound();
            }
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("register")]
        public async Task<ActionResult<Admin>> AddUser(AdminDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var newAdmin = new Admin()
            {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
            };
            await _context.Admins.AddAsync(newAdmin);
            await _context.SaveChangesAsync();

            return newAdmin;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(AdminDto request)
        {
            var existingUser = await _context.Admins.FirstOrDefaultAsync(r => r.UserName == request.UserName);

            if (existingUser == null)
            {
                return BadRequest("Admin don't found");
            }

            if (!VerifyPasswordHash(request.Password, existingUser.PasswordHash, existingUser.PasswordSalt))
            {
                return BadRequest("Wrong Password");
            }

            string token = CreateToken(existingUser);
            return Ok(token);
        }

        [HttpPost("response")]
        public async Task<ActionResult<FacultiesResponse>> AddResponse(FacultiesResponse response)
        {
            if (response == null)
            {
                return BadRequest();
            }

            await _context.Responses.AddAsync(response);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("response")]
        public async Task<ActionResult<FacultiesResponse>> GetRespons(string facName)
        {
            var resp = _context.Responses.FindAsync(facName).Result;
            if (resp == null)
            {
                return NotFound();
            }

            return resp;
        }

        [HttpPut("response")]
        public async Task<ActionResult<FacultiesResponse>> UpdateResponse(FacultiesResponse response)
            {
            if (response == null)
            {
                return BadRequest();
            }

            if (_context.Responses.Any(r => r.FacultyName == response.FacultyName))
            {
                var upResponse = await _context.Responses.FindAsync(response.FacultyName);
                upResponse.ImageUrl = response.ImageUrl;
                upResponse.Author = response.Author;

                _context.Responses.Update(upResponse);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("upload")]
        public async Task<ActionResult> Upload([FromBody]FireUploadViewModel files)
        {
            return UploadFile(files, "images").Result;
        }

        //private async Task<ActionResult> UploadFile(List<IFormFile> files, string folderName)
        //{
        //    var fileupload = files[0];

        //    var uploadResult = new UploadResult();
        //    string trustedFileNameForFileStorage;
        //    var untrustedFileName = fileupload.FileName;
        //    uploadResult.FileName = untrustedFileName;
        //    var trustedFilenNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);

        //    trustedFileNameForFileStorage = Path.GetRandomFileName();
        //    var path = Path.Combine(_env.ContentRootPath,"uploads", trustedFileNameForFileStorage);

        //    await using FileStream fs = new(path, FileMode.Create);
        //    uploadResult.StoredFileName = trustedFileNameForFileStorage;



        //    return Ok(uploadResult);
        //}

        private async Task<ActionResult> UploadFile(FireUploadViewModel file, string folderName)
        {
            var fileupload = file.File;
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
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Create);
                    await fileupload.CopyToAsync(stream);
                    stream.Close();
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Open);
                }
                else
                {
                    Directory.CreateDirectory(path);
                    stream = new FileStream(Path.Combine(path, fileupload.FileName), FileMode.Create);
                    await fileupload.CopyToAsync(stream);
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
                    .Child(folderName)
                    .Child(DateTime.Now.ToString())
                    .Child($"{fileupload.FileName}.{Path.GetExtension(fileupload.FileName).Substring(1)}")
                    .PutAsync(stream, cancellation.Token);

                try
                {
                    string link = await task;
                    stream.Close();
                    Directory.Delete(path, true);
                    return Ok(link);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    stream.Close();
                    throw;
                }

            }
            return BadRequest();
        }

        private string CreateToken(Admin admin)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,admin.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            return jwt;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            };
        }

        private bool AdminExists(string id)
        {
            return (_context.Admins?.Any(e => e.UserName == id)).GetValueOrDefault();
        }
    }
}
