public class Office
{
    public int Id { get; set; }
    public string OfficeNumber { get; set; } = default!;

    public int ProfessorId { get; set; }
    public Professor Professor { get; set; } = default!;
}