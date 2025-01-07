
namespace Dogs.Services;

public record Dog(string Breed, int Age);

 public interface IDogsService
{
    Task<Dog[]> GetDogs(string breed);
}

public class DogsService : IDogsService
{
    private readonly string[] _breeds = ["Chihuahua", "Corgi", "Doberman", "Spaniel"];

    public async Task<Dog[]> GetDogs(string breed)
    {
        var dogs = _breeds.Where(x => x.Equals(breed, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => new Dog(x, new Random().Next(10, 15))).ToArray();

        return await Task.FromResult(dogs);
    }
}
