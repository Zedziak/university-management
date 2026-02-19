using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Services
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        protected readonly UniversityDbContext Context;
        public DbSet<T> DbSet { get; }

        public EfRepository(UniversityDbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public Task<T?> GetByIdAsync(object id)
        {
            return DbSet.FindAsync(id).AsTask();
        }

        public Task<T?> GetByIndexAsync(string index)
        {
            if (typeof(Person).IsAssignableFrom(typeof(T)))
            {
                return DbSet.FirstOrDefaultAsync(e => ((Person)(object)e).UniversityIndex == index);
            }
            return Task.FromResult<T?>(null);
        }

        public async Task<T> AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
