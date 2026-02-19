using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EfQueryRepository : IQueryRepository
    {
        private readonly UniversityDbContext _context;

        public EfQueryRepository(UniversityDbContext context)
        {
            _context = context;
        }

        public async Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Select(s => new
                {
                    FullName = s.FirstName + " " + s.LastName,
                    CurrentEctsSum = s.Enrollments.Sum(e => e.Course.EctsPoints),
                    PrereqEctsSum = s.Enrollments
                        .SelectMany(e => e.Course.Prerequisites)
                        .Distinct()
                        .Sum(prereq => prereq.EctsPoints)
                })
                .Select(s => new StudentDifficultyDto
                {
                    FullName = s.FullName,
                    CurrentEctsSum = s.CurrentEctsSum,
                    PrereqEctsSum = s.PrereqEctsSum,
                    TotalDifficulty = s.CurrentEctsSum + s.PrereqEctsSum
                })
                .OrderByDescending(s => s.TotalDifficulty)
                .FirstOrDefaultAsync();
        }

        public async Task<TopProfessorDto?> GetTopProfessorByStudentsAsync()
        {
            return await _context.Professors
                .AsNoTracking()
                .Select(p => new
                {
                    ProfessorName = p.FirstName + " " + p.LastName,
                    TotalStudents = _context.Courses
                        .Where(c => c.CourseCode != null)
                        .SelectMany(c => c.Enrollments)
                        .Count()
                })
                .OrderByDescending(result => result.TotalStudents)
                .Select(p => new TopProfessorDto
                {
                    ProfessorName = p.ProfessorName,
                    TotalStudents = p.TotalStudents
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CourseGpaDto>> GetGpaPerCourseForDepartmentAsync(string departmentName)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.Department.Name == departmentName)
                .Where(c => c.Enrollments.Any(e => e.Grade > 0))
                .Select(c => new CourseGpaDto
                {
                    CourseName = c.Name,
                    AverageGpa = (double)c.Enrollments
                        .Where(e => e.Grade > 0)
                        .Average(e => e.Grade),

                    StudentCount = c.Enrollments
                        .Count(e => e.Grade > 0)
                })
                .ToListAsync();
        }
    }
}
