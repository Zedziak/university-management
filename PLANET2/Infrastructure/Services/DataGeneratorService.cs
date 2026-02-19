using Application.Services;
using Bogus;
using Infrastructure.Persistence;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DataGeneratorService
    {
        private readonly PersonService _personService;
        private readonly IUnitOfWork _uow;
        private readonly UniversityDbContext _context;

        public DataGeneratorService(PersonService personService, IUnitOfWork uow, UniversityDbContext context)
        {
            _personService = personService;
            _uow = uow;
            _context = context;
        }

        public async Task GenerateData(int studentCount, int professorCount)
        {
            var addressFaker = new Faker<Address>()
                .CustomInstantiator(f => new Address(
                    f.Address.StreetAddress(),
                    f.Address.City(),
                    f.Address.ZipCode()
                ));

            var departments = new List<Department>
        {
            new Department { Name = "ETI" },
            new Department { Name = "EIA" },
            new Department { Name = "WFTIMS" }
        };
            await _context.Departments.AddRangeAsync(departments);
            await _context.SaveChangesAsync();

            var professorFaker = new Faker<Professor>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.AcademicTitle, f => f.PickRandom(new[] { "Dr inż.", "Prof. Dr Hab.", "Prof.", "Mgr inż." }))
                .RuleFor(p => p.ResidentialAddress, f => addressFaker.Generate());

            var professors = professorFaker.Generate(professorCount);

            foreach (var prof in professors)
            {
                await _personService.CreatePersonAsync(prof, "P");
                var office = new Office
                {
                    OfficeNumber = "A" + new Random().Next(100, 999),
                    ProfessorId = prof.Id
                };
                await _context.Offices.AddAsync(office);
            }
            await _uow.SaveChangesAsync();

            var studentFaker = new Faker<Student>()
                .RuleFor(s => s.FirstName, f => f.Name.FirstName())
                .RuleFor(s => s.LastName, f => f.Name.LastName())
                .RuleFor(s => s.StudyYear, f => f.Random.Int(1, 5))
                .RuleFor(s => s.ResidentialAddress, f => addressFaker.Generate());

            var students = studentFaker.Generate(studentCount);

            foreach (var student in students)
            {
                await _personService.CreatePersonAsync(student, "S");
            }
            await _uow.SaveChangesAsync();

            var courses = new Faker<Course>()
                .RuleFor(c => c.Name, f => f.Hacker.Noun())
                .RuleFor(c => c.CourseCode, f => f.Random.Replace("K###"))
                .RuleFor(c => c.EctsPoints, f => f.Random.Int(3, 8))
                .RuleFor(c => c.DepartmentId, f => f.PickRandom(departments).Id)
                .Generate(20);

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            var studentsList = await _context.Students.ToListAsync();

            var random = new Random();
            foreach (var student in studentsList)
            {
                var enrolledCourses = courses.OrderBy(x => random.Next()).Take(random.Next(2, 6));

                foreach (var course in enrolledCourses)
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        Semester = $"Semestr {random.Next(1, 8)}",
                        Grade = (decimal)(2.0 + (random.NextDouble() * 3.0))
                    };
                    await _context.Enrollments.AddAsync(enrollment);
                }
            }

            if (courses.Count > 1)
            {
                courses[1].Prerequisites.Add(courses[0]);
                courses[2].Prerequisites.Add(courses[1]);
            }

            await _context.SaveChangesAsync();
        }
    }
}
