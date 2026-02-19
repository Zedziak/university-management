using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Person> People { get; }
        IRepository<Student> Students { get; }
        IRepository<Professor> Professors { get; }
        IRepository<IndexCounter> IndexCounters { get; }
        IRepository<Enrollment> Enrollments { get; }
        IRepository<Office> Offices { get; }

        Task<Person?> GetPersonWithDetailsAsync(int id);
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

        Task<int> SaveChangesAsync();
    }
}
