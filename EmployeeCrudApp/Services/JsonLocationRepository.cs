using System.Text.Json;
using EmployeeCrudApp.Models;

namespace EmployeeCrudApp.Services;

public class JsonLocationRepository : ILocationRepository
{
    private readonly string _filePath;

    public JsonLocationRepository(IWebHostEnvironment webHostEnvironment)
    {
        _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "locations.json");
    }

    private async Task<List<Location>> LoadAllAsync()
    {
        if (!File.Exists(_filePath)) return new List<Location>();
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Location>>(json) ?? new List<Location>();
    }

    private async Task SaveAllAsync(List<Location> locations)
    {
        var json = JsonSerializer.Serialize(locations, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<IEnumerable<Location>> GetAllAsync() => await LoadAllAsync();

    public async Task<Location?> GetByIdAsync(int id)
    {
        var locations = await LoadAllAsync();
        return locations.FirstOrDefault(l => l.Id == id);
    }

    public async Task AddAsync(Location location)
    {
        var locations = await LoadAllAsync();
        location.Id = locations.Any() ? locations.Max(l => l.Id) + 1 : 1;
        locations.Add(location);
        await SaveAllAsync(locations);
    }

    public async Task UpdateAsync(Location location)
    {
        var locations = await LoadAllAsync();
        var index = locations.FindIndex(l => l.Id == location.Id);
        if (index != -1)
        {
            locations[index] = location;
            await SaveAllAsync(locations);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var locations = await LoadAllAsync();
        var location = locations.FirstOrDefault(l => l.Id == id);
        if (location != null)
        {
            locations.Remove(location);
            await SaveAllAsync(locations);
        }
    }

    public async Task SeedCountriesAsync(IEnumerable<Location> countries)
    {
        var locations = await LoadAllAsync();
        // Only add countries that don't exist by name
        var existingNames = locations.Where(l => l.Type == LocationType.Country).Select(l => l.Name.ToLower()).ToHashSet();
        
        int nextId = locations.Any() ? locations.Max(l => l.Id) + 1 : 1;
        foreach (var country in countries)
        {
            if (!existingNames.Contains(country.Name.ToLower()))
            {
                country.Id = nextId++;
                locations.Add(country);
            }
        }
        await SaveAllAsync(locations);
    }
}
