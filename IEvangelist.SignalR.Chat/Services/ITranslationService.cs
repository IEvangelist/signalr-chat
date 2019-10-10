using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public interface ITranslationService
    {
        ValueTask<(string text, bool isTranslated)> TranslateAsync(string text, string lang);
    }
}