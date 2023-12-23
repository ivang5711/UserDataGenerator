namespace UserRegistry.Client.Models;

public record PersonModel
{
    public required int Number { get; set; }
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string Phone { get; set; }
}