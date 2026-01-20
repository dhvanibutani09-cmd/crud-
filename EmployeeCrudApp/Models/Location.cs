using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public enum LocationType
{
    Country,
    City,
    Village
}

public class Location
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public LocationType Type { get; set; }

    [Display(Name = "Country Code")]
    public string? CountryCode { get; set; }

    [Display(Name = "Parent Location")]
    public int? ParentLocationId { get; set; }
}
