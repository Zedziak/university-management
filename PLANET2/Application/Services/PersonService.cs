using Application.Interfaces;

namespace Application.Services
{
    public class PersonService
    {
        private readonly IIndexCounterService _counterService;
        private readonly IUnitOfWork _uow;

        public PersonService(IIndexCounterService counterService, IUnitOfWork uow)
        {
            _counterService = counterService;
            _uow = uow;
        }

        public async Task CreatePersonAsync(Person person, string prefix)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var result = await _counterService.GetAndIncrementIndexAsync(prefix, _uow);
                person.UniversityIndex = result.Index;

                if (prefix == "S")
                    await _uow.Students.AddAsync((Student)person);
                else if (prefix == "P")
                    await _uow.Professors.AddAsync((Professor)person);

                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task DeletePersonAsync(int personId)
        {
            var personToDelete = await _uow.GetPersonWithDetailsAsync(personId);

            if (personToDelete == null) return;

            await _uow.BeginTransactionAsync();
            try
            {
                string prefix = personToDelete.UniversityIndex.Substring(0, 1);

                await _counterService.ShouldDecrementAndDecrementAsync(
                    prefix, personToDelete.UniversityIndex, _uow);

                if (personToDelete is Student student && student.Enrollments != null)
                {
                    foreach (var enrollment in student.Enrollments.ToList())
                    {
                        await _uow.Enrollments.DeleteAsync(enrollment);
                    }
                }

                if (personToDelete is Professor professor && professor.Office != null)
                {
                    await _uow.Offices.DeleteAsync(professor.Office);
                }

                await _uow.People.DeleteAsync(personToDelete);

                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
    }
}
