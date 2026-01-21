using EmployeeCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

            var results = await _translationService.TranslateBatchAsync(request.Texts, request.TargetLanguage);
            return Ok(results);
        }
    }

    public class TranslationRequest
    {
        public List<string> Texts { get; set; }
        public string TargetLanguage { get; set; }
    }
}
