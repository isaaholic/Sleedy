using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class ServerContext:DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options):base(options)
        {

        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<FacultiesResponse> Responses { get; set; }
    }
}
