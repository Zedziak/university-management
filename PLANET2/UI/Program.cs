using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.UOW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Error);
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<UniversityDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IQueryRepository, EfQueryRepository>();
        services.AddScoped<IIndexCounterService, IndexCounterService>();

        services.AddScoped<PersonService>();
        services.AddScoped<QueryService>();
        services.AddScoped<DataGeneratorService>();
    })
    .Build();

await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<UniversityDbContext>();
    var uow = services.GetRequiredService<IUnitOfWork>();
    var licznikService = services.GetRequiredService<IIndexCounterService>();

    try
    {
        dbContext.Database.Migrate();

        if (await uow.IndexCounters.GetByIdAsync("S") == null)
        {
            await licznikService.AddNewPrefixAsync("S", 1001, uow);
        }
        if (await uow.IndexCounters.GetByIdAsync("P") == null)
        {
            await licznikService.AddNewPrefixAsync("P", 101, uow);
        }
        await uow.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"BŁĄD KRYTYCZNY (MIGRACJA): {ex.Message}");
        Console.ReadKey();
    }

    var generator = services.GetRequiredService<DataGeneratorService>();
    var queryService = services.GetRequiredService<QueryService>();
    var personService = services.GetRequiredService<PersonService>();

    bool running = true;
    while (running)
    {
        Console.Clear();
        Console.WriteLine("1. Generuj dane testowe");
        Console.WriteLine("2. Wyświetl studentów");
        Console.WriteLine("3. Wyświetl profesorów");
        Console.WriteLine("4. Wyświetl kursy");
        Console.WriteLine("5. Wyświetl wydziały");
        Console.WriteLine("6. Wyświetl liczniki");
        Console.WriteLine("7. Usuń wybraną osobę");
        Console.WriteLine("8. Wykonaj zapytania (Zad. 4)");
        Console.WriteLine("0. Wyjdź");
        Console.Write("Wybierz opcję: ");

        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            try
            {
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Generowanie danych...");
                        await generator.GenerateData(studentCount: 50, professorCount: 10);
                        Console.WriteLine("GOTOWE");
                        break;
                    case 2:
                        await DisplayStudents(dbContext);
                        break;
                    case 3:
                        await DisplayProfessors(dbContext);
                        break;
                    case 4:
                        await DisplayCourses(dbContext);
                        break;
                    case 5:
                        await DisplayDepartments(dbContext);
                        break;
                    case 6:
                        await DisplayCounters(uow);
                        break;
                    case 7:
                        await DeleteSpecificPerson(personService);
                        break;
                    case 8:
                        await RunComplexQueries(queryService, uow);
                        break;
                    case 0:
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Brak opcji");
                        break;
                }

                if (choice != 0)
                {
                    Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić do menu...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nBłąd wykonania: {ex.Message}");
                Console.WriteLine("\nNaciśnij dowolny klawisz...");
                Console.ReadKey();
            }
        }
    }
}
await host.StopAsync();

async Task DisplayStudents(UniversityDbContext context)
{
    var students = await context.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Id,
            s.UniversityIndex,
            Name = s.FirstName + " " + s.LastName,
            City = s.ResidentialAddress.City,
            s.StudyYear
        })
        .ToListAsync();

    if (!students.Any())
    {
        Console.WriteLine("Brak studentów w bazie.");
        return;
    }

    Console.WriteLine($"{"ID",-5}{"INDEX",-10}{"NAZWISKO I IMIĘ",-30}{"MIASTO",-20}{"ROK",-5}");
    Console.WriteLine(new string('-', 75));

    foreach (var s in students)
    {
        Console.WriteLine($"{s.Id,-5}{s.UniversityIndex,-10}{s.Name,-30}{s.City,-20}{s.StudyYear,-5}");
    }
    Console.WriteLine($"\nRazem: {students.Count} studentów.");
}

async Task DisplayDepartments(UniversityDbContext context)
{
    var departments = await context.Departments
        .AsNoTracking()
        .Select(d => new { d.Id, d.Name })
        .ToListAsync();

    if (!departments.Any())
    {
        Console.WriteLine("Brak wydziałów w bazie.");
        return;
    }

    Console.WriteLine($"{"ID",-5}{"NAZWA WYDZIAŁU",-30}");
    Console.WriteLine(new string('-', 35));

    foreach (var d in departments)
    {
        Console.WriteLine($"{d.Id,-5}{d.Name,-30}");
    }
}

async Task DisplayProfessors(UniversityDbContext context)
{
    var professors = await context.Professors
        .AsNoTracking()
        .Select(p => new
        {
            p.Id,
            p.UniversityIndex,
            Name = p.FirstName + " " + p.LastName,
            p.AcademicTitle,
            OfficeNumber = p.Office != null ? p.Office.OfficeNumber : "Brak"
        })
        .ToListAsync();

    if (!professors.Any())
    {
        Console.WriteLine("Brak profesorów w bazie.");
        return;
    }

    Console.WriteLine($"{"ID",-5}{"INDEX",-10}{"TYTUŁ I NAZWISKO",-35}{"GABINET",-10}");
    Console.WriteLine(new string('-', 65));

    foreach (var p in professors)
    {
        Console.WriteLine($"{p.Id,-5}{p.UniversityIndex,-10}{p.AcademicTitle + " " + p.Name,-35}{p.OfficeNumber,-10}");
    }
    Console.WriteLine($"\nRazem: {professors.Count} profesorów.");
}

async Task DeleteSpecificPerson(PersonService personService)
{
    Console.Write("Podaj ID osoby do usunięcia: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine($"Próba usunięcia osoby o ID: {id}...");

        try
        {
            await personService.DeletePersonAsync(id);
            Console.WriteLine("Usunięto");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas usuwania: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Nieprawidłowe ID.");
    }
}

async Task DisplayCourses(UniversityDbContext context)
{
    var courses = await context.Courses
        .AsNoTracking()
        .Select(c => new
        {
            c.Id,
            c.CourseCode,
            c.Name,
            c.EctsPoints,
            DepartmentName = c.Department.Name
        })
        .OrderBy(c => c.DepartmentName)
        .ThenBy(c => c.Name)
        .ToListAsync();

    if (!courses.Any())
    {
        Console.WriteLine("Brak kursów w bazie.");
        return;
    }

    Console.WriteLine($"{"KOD",-10}{"NAZWA KURSU",-35}{"ECTS",-5}{"WYDZIAŁ",-20}");
    Console.WriteLine(new string('-', 75));

    foreach (var c in courses)
    {
        Console.WriteLine($"{c.CourseCode,-10}{c.Name,-35}{c.EctsPoints,-5}{c.DepartmentName,-20}");
    }
    Console.WriteLine($"\nRazem: {courses.Count} kursów.");
}

async Task DisplayCounters(IUnitOfWork uow)
{
    var counters = await ((EfRepository<IndexCounter>)uow.IndexCounters).DbSet
        .AsNoTracking()
        .ToListAsync();

    Console.WriteLine($"{"PREFIX",-10}{"AKTUALNA WARTOŚĆ",-25}");
    Console.WriteLine(new string('-', 35));

    foreach (var c in counters)
    {
        Console.WriteLine($"{c.Prefix,-10}{c.CurrentValue,-25}");
    }
}

async Task RunComplexQueries(QueryService queryService, IUnitOfWork uow)
{
    Console.WriteLine("\n1. Student z najtrudniejszym planem:");
    var hardestStudent = await queryService.GetStudentWithHardestSchedule();
    if (hardestStudent != null)
    {
        Console.WriteLine($"   Student:  {hardestStudent.FullName}");
        Console.WriteLine($"   Trudność: {hardestStudent.TotalDifficulty} ECTS (Bieżące: {hardestStudent.CurrentEctsSum}, Prereq: {hardestStudent.PrereqEctsSum})");
    }
    else
    {
        Console.WriteLine("   Brak danych.");
    }

    Console.WriteLine("\n2. Profesor z największą liczbą studentów:");
    var topProf = await queryService.GetTopProfessorByStudents();
    if (topProf != null)
    {
        Console.WriteLine($"   Profesor: {topProf.ProfessorName}");
        Console.WriteLine($"   Studentów: {topProf.TotalStudents}");
    }
    else
    {
        Console.WriteLine("   Brak danych.");
    }

    Console.WriteLine("\n3. Średnia ocen dla kursów na wydziale:");
    Console.Write("   Wpisz nazwę wydziału: ");
    string deptName = Console.ReadLine();

    var results = await queryService.GetGpaPerCourseForDepartment(deptName);
    if (results.Any())
    {
        Console.WriteLine($"\n   Wyniki dla wydziału: {deptName}");
        Console.WriteLine($"   {"NAZWA KURSU",-30} {"ŚREDNIA",-10} {"STUDENTÓW",-10}");
        Console.WriteLine(new string('-', 55));
        foreach (var r in results)
        {
            Console.WriteLine($"   {r.CourseName,-30} {r.AverageGpa,-10:F2} {r.StudentCount,-10}");
        }
    }
    else
    {
        Console.WriteLine($"   Brak wyników dla wydziału '{deptName}' lub brak ocen.");
    }
}