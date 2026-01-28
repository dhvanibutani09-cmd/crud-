
using EmployeeCrudApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeCrudApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (request == null || request.Texts == null || !request.Texts.Any())
            {
                return BadRequest("Invalid request");
            }

            var results = await _translationService.TranslateBatchAsync(request.Texts, request.TargetLanguage, request.SourceLanguage);
            return Ok(results);
        }

        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _translationService.GetLanguagesAsync();
            return Ok(languages);
        }
    }

    public class TranslationRequest
    {
        public List<string> Texts { get; set; }
        public string TargetLanguage { get; set; }
        public string SourceLanguage { get; set; }
    }
}
