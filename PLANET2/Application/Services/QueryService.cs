using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class QueryService
    {
        private readonly IQueryRepository _queryRepository;

        public QueryService(IQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public Task<StudentDifficultyDto?> GetStudentWithHardestSchedule()
        {
            return _queryRepository.GetStudentWithHardestScheduleAsync();
        }

        public Task<TopProfessorDto?> GetTopProfessorByStudents()
        {
            return _queryRepository.GetTopProfessorByStudentsAsync();
        }

        public Task<IEnumerable<CourseGpaDto>> GetGpaPerCourseForDepartment(string departmentName)
        {
            return _queryRepository.GetGpaPerCourseForDepartmentAsync(departmentName);
        }
    }
}
