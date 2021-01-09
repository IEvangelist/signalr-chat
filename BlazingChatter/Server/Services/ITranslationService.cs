using System.Threading.Tasks;

namespace BlazingChatter.Services
{
    public interface ITranslationService
    {
        ValueTask<(string text, bool isTranslated)> TranslateAsync(string text, string lang);
    }
}