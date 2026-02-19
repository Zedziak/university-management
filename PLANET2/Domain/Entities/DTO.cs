namespace Domain.Entities
{
    public class StudentDifficultyDto
    {
        public string FullName { get; set; } = string.Empty;
        public int TotalDifficulty { get; set; }
        public int CurrentEctsSum { get; set; }
        public int PrereqEctsSum { get; set; }
    }

    public class TopProfessorDto
    {
        public string ProfessorName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
    }

    public class CourseGpaDto
    {
        public string CourseName { get; set; } = string.Empty;
        public double AverageGpa { get; set; }
        public int StudentCount { get; set; }
    }
}
