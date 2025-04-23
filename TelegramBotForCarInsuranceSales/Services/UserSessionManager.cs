using System.Collections.Concurrent;
using TelegramBotForCarInsuranceSales.Models;

public class UserSessionManager
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    // Retrieves the existing session or creates a new one if it doesn't exist
    public UserSession GetOrCreateSession(long chatId)
    {
        return _sessions.GetOrAdd(chatId, new UserSession());
    }
}