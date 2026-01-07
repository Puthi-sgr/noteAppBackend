using Microsoft.EntityFrameworkCore;
using NoteApp.Domain;

namespace NoteApp.Data
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //This is the part that tells ef core that these domain are entities that will be mapped to the database tables
        public DbSet<Note> Notes => Set<Note>(); 
        public DbSet<User> Users => Set<User>();
        public DbSet<Comment> Comments => Set<Comment>();


    }
}