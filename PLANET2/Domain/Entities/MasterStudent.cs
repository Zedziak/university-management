public class MasterStudent : Student
{
    public string? ThesisTopic { get; set; }
    public int? SupervisorId { get; set; }
    public Professor? Supervisor { get; set; }
}