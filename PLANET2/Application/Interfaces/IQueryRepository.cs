using Domain.Entities;

namespace Application.Interfaces
{
    public interface IQueryRepository
    {
        Task<TopProfessorDto?> GetTopProfessorByStudentsAsync();
        Task<IEnumerable<CourseGpaDto>> GetGpaPerCourseForDepartmentAsync(string departmentName);
        Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync();
    }
}