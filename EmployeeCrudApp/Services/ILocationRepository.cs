using EmployeeCrudApp.Models;

namespace EmployeeCrudApp.Services;

public interface ILocationRepository
{
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(int id);
    Task SeedCountriesAsync(IEnumerable<Location> countries);
}
