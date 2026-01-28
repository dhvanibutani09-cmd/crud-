namespace EmployeeCrudApp.Services
{
    public class LanguageInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage = "auto");
        Task<Dictionary<string, string>> TranslateBatchAsync(List<string> texts, string targetLanguage, string sourceLanguage = "auto");
        Task<List<LanguageInfo>> GetLanguagesAsync();
    }
}
