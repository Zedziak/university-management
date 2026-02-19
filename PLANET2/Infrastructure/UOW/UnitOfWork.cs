using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UniversityDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRepository<Person> People { get; }
        public IRepository<Student> Students { get; }
        public IRepository<Professor> Professors { get; }
        public IRepository<IndexCounter> IndexCounters { get; }
        public IRepository<Enrollment> Enrollments { get; }
        public IRepository<Office> Offices { get; }

        public UnitOfWork(UniversityDbContext context)
        {
            _context = context;
            People = new EfRepository<Person>(context);
            Students = new EfRepository<Student>(context);
            Professors = new EfRepository<Professor>(context);
            IndexCounters = new EfRepository<IndexCounter>(context);
            Enrollments = new EfRepository<Enrollment>(context);
            Offices = new EfRepository<Office>(context);
        }

        public async Task<Person?> GetPersonWithDetailsAsync(int id)
        {
            return await _context.People
                .Include(p => ((Student)p).Enrollments)
                .Include(p => ((Professor)p).Office)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null) return;
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction!.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) return;
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
