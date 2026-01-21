namespace EmployeeCrudApp.Services
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string targetLanguage);
        Task<Dictionary<string, string>> TranslateBatchAsync(List<string> texts, string targetLanguage);
    }
}
