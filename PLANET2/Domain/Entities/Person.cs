public abstract class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public Address ResidentialAddress { get; set; } = default!;
}