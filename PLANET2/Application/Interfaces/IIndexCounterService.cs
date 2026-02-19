using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IIndexCounterService
    {
        Task<(string Index, int NewValue)> GetAndIncrementIndexAsync(string prefix, IUnitOfWork uow);
        Task<bool> ShouldDecrementAndDecrementAsync(string currentPrefix, string currentIndex, IUnitOfWork uow);
        Task AddNewPrefixAsync(string prefix, int initialValue, IUnitOfWork uow);
    }
}