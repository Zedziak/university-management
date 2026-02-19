using Application.Interfaces;

namespace Infrastructure.Services
{
    public class IndexCounterService : IIndexCounterService
    {
        public async Task<(string Index, int NewValue)> GetAndIncrementIndexAsync(string prefix, IUnitOfWork uow)
        {
            var counter = await uow.IndexCounters.GetByIdAsync(prefix);
            if (counter == null) throw new InvalidOperationException($"Counter for prefix '{prefix}' does not exist.");

            string nextIndex = $"{prefix}{counter.CurrentValue}";
            counter.CurrentValue++;
            await uow.IndexCounters.UpdateAsync(counter);

            return (nextIndex, counter.CurrentValue);
        }

        public async Task<bool> ShouldDecrementAndDecrementAsync(string currentPrefix, string currentIndex, IUnitOfWork uow)
        {
            var counter = await uow.IndexCounters.GetByIdAsync(currentPrefix);
            if (counter == null) return false;

            int nextExpectedValue = counter.CurrentValue;

            if (int.Parse(currentIndex.Substring(1)) == nextExpectedValue - 1)
            {
                counter.CurrentValue--;
                await uow.IndexCounters.UpdateAsync(counter);
                return true;
            }
            return false;
        }

        public async Task AddNewPrefixAsync(string prefix, int initialValue, IUnitOfWork uow)
        {
            var newCounter = new IndexCounter { Prefix = prefix, CurrentValue = initialValue };
            await uow.IndexCounters.AddAsync(newCounter);
        }
    }
}
