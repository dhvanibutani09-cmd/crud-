using Microsoft.AspNetCore.Mvc;
using EmployeeCrudApp.Models;
using EmployeeCrudApp.Services;
using System.Net.Http;
using System.Text.Json;

namespace EmployeeCrudApp.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public LocationController(ILocationRepository locationRepository, IHttpClientFactory httpClientFactory)
        {
            _locationRepository = locationRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _locationRepository.GetAllAsync();
            return View(locations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Location location)
        {
            if (ModelState.IsValid)
            {
                await _locationRepository.AddAsync(location);
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                await _locationRepository.UpdateAsync(location);
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _locationRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ImportCountries()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://restcountries.com/v3.1/all?fields=name,cca2");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var countryData = JsonDocument.Parse(content);
                    var countries = new List<Location>();

                    foreach (var element in countryData.RootElement.EnumerateArray())
                    {
                        countries.Add(new Location
                        {
                            Name = element.GetProperty("name").GetProperty("common").GetString() ?? "",
                            Type = LocationType.Country,
                            CountryCode = element.GetProperty("cca2").GetString()
                        });
                    }

                    await _locationRepository.SeedCountriesAsync(countries);
                    return Json(new { success = true, count = countries.Count });
                }
                return Json(new { success = false, message = "API request failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
