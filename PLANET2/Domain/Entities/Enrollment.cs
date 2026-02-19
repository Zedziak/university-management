public class Enrollment
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public string Semester { get; set; } = default!;
    public decimal Grade { get; set; }
}