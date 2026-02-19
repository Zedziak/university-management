public class Student : Person
{
    public int StudyYear { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}