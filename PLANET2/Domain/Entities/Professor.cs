public class Professor : Person
{
    public string AcademicTitle { get; set; } = string.Empty;
    public Office? Office { get; set; }
    public ICollection<MasterStudent> SupervisedStudents { get; set; } = new List<MasterStudent>();
}