using System.Threading.Tasks;

namespace BlazorR.Chat.Services
{
    public interface ITranslationService
    {
        ValueTask<(string text, bool isTranslated)> TranslateAsync(string text, string lang);
    }
}