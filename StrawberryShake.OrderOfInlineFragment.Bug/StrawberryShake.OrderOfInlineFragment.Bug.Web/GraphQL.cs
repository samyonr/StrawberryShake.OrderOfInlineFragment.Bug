namespace StrawberryShake.OrderOfInlineFragment.Bug.Web;

public class Query
{
    public IEnumerable<Person> GetPeople() =>
    [
        new()
        {
            Id = 1,
            Name = "Alice",
            Pet = new Dog { Id = 101, Breed = "Bulldog" }
        },
        new()
        {
            Id = 2,
            Name = "Bob",
            Pet = new Cat { Id = 102, FurColor = "Black" }
        }
    ];

    public IEnumerable<Shelter> GetShelters() =>
    [
        new()
        {
            Id = 1,
            Location = "Downtown",
            Pet = new Bird { Id = 201, WingSpan = 42 }
        },
        new()
        {
            Id = 2,
            Location = "Uptown",
            Pet = new Dog { Id = 202, Breed = "Labrador" }
        }
    ];
}

// Interface + implementing types
public interface IPet
{
    int Id { get; }
}

public class Dog : IPet
{
    public int Id { get; set; }
    public string Breed { get; set; } = null!;
}

public class Cat : IPet
{
    public int Id { get; set; }
    public string FurColor { get; set; } = null!;
}

public class Bird : IPet
{
    public int Id { get; set; }
    public int WingSpan { get; set; }
}

// 'Host' objects
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IPet? Pet { get; set; }
}

public class Shelter
{
    public int Id { get; set; }
    public string Location { get; set; } = null!;
    public IPet? Pet { get; set; }
}