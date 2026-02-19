public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int EctsPoints { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Course> RequiredBy { get; set; } = new List<Course>();
    public ICollection<Course> Prerequisites { get; set; } = new List<Course>();
}