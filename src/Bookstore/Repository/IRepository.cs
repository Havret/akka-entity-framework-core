using System.Threading.Tasks;

namespace Bookstore.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<T> FindAsync(object[] keyValues);
        T Add(T entity);
        T Update(T entity);
        Task SaveAsync();
    }
}