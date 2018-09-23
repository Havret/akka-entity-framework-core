using Bookstore.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Repository
{
    public class BookstoreContext : DbContext
    {
        public BookstoreContext(DbContextOptions<BookstoreContext> options) : base(options) { }
        public DbSet<Book> Books { get; set; }
    }
}