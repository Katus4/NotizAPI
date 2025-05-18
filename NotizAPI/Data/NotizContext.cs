using Microsoft.EntityFrameworkCore;
using NotizApi.Models;

namespace NotizApi.Data
{
    public class NotizContext : DbContext
    {
        public NotizContext(DbContextOptions<NotizContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}