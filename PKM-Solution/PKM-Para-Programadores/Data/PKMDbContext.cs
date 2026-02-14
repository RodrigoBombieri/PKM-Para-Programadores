using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PKM_Project.Models;

namespace PKM_Project.Data
{
    public class PKMDbContext : IdentityDbContext<Usuario>
    {
        public PKMDbContext(DbContextOptions<PKMDbContext> options) : base(options)
        {
        }
    }
}
