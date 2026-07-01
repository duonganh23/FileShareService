using Microsoft.EntityFrameworkCore;
using FileShareService.Models;
namespace FileShareService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<FileRecord> Files { get; set; }
    }
}
