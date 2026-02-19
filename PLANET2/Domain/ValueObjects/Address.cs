public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }

    public Address(string street, string city, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.");

        Street = street;
        City = city;
        PostalCode = postalCode;
    }
}