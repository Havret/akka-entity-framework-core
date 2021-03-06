﻿using System.Threading.Tasks;

namespace Bookstore.Repository
{
    class BookstoreRepository<T> : IRepository<T> where T : class
    {
        private readonly BookstoreContext _dbContext;

        public BookstoreRepository(BookstoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> FindAsync(object[] keyValues)
        {
            return await _dbContext.Set<T>().FindAsync(keyValues);
        }

        public T Add(T entity)
        {
            var entityEntry = _dbContext.Set<T>().Add(entity);
            return entityEntry.Entity;
        }

        public T Update(T entity)
        {
            var entityEntry = _dbContext.Set<T>().Update(entity);
            return entityEntry.Entity;
        }

        public T Remove(T entity)
        {
            var entityEntry = _dbContext.Set<T>().Remove(entity);
            return entityEntry.Entity;
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}