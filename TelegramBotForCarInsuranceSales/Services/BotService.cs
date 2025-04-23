using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotForCarInsuranceSales.Models;

public class BotService
{
    private readonly TelegramBotClient _botClient;
    private readonly UserSessionManager _sessionManager;
    private readonly AIConversationService _aiService;
    private readonly DocumentProcessorService _documentProcessor;
    private readonly PolicyGeneratorService _policyGenerator;

    public BotService(AIConversationService aiService)
    {
        _botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"));
        _sessionManager = new UserSessionManager();
        _aiService = aiService;
        _documentProcessor = new DocumentProcessorService();
        _policyGenerator = new PolicyGeneratorService();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // No specific update types are filtered, receive all updates
        };

        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"Bot {me.Username} is up and running");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message == null) return;

        var chatId = update.Message.Chat.Id;
        var session = _sessionManager.GetOrCreateSession(chatId);

        // Handling "/start" command
        if (update.Message.Text?.ToLower() == "/start")
        {
            session.Reset();
            session.Stage = ConversationStage.WaitingForPassport;
            await Respond(chatId, "start", session, cancellationToken);
            return;
        }

        // Handle photo uploads
        if (update.Message.Photo?.Any() == true)
        {
            var photo = update.Message.Photo.Last();
            if (session.Stage == ConversationStage.WaitingForPassport)
            {
                session.PassportPhotoFileId = photo.FileId;
                session.Stage = ConversationStage.WaitingForVehicleDoc;
                await Respond(chatId, "passport_received", session, cancellationToken);
            }
            else if (session.Stage == ConversationStage.WaitingForVehicleDoc)
            {
                session.VehiclePhotoFileId = photo.FileId;
                session.Stage = ConversationStage.ConfirmingData;

                // Extract data from documents
                session.ExtractedData = await _documentProcessor.ExtractData(session.PassportPhotoFileId, session.VehiclePhotoFileId);

                var preview = string.Join("\n", session.ExtractedData.Select(kv => $"{kv.Key}: {kv.Value}"));
                await Respond(chatId, $"extracted_data:\n{preview}", session, cancellationToken);
            }
            return;
        }
        else if (!string.IsNullOrEmpty(update.Message.Text))
        {
            // Handle text messages
            var userTextPhoto = update.Message.Text.ToLower();

            if (session.Stage == ConversationStage.WaitingForPassport)
            {
                // If a text message is sent while waiting for the passport photo
                await Respond(chatId, "Please upload a photo of your passport 📷", session, cancellationToken);
            }
            else if (session.Stage == ConversationStage.WaitingForVehicleDoc)
            {
                // If a text message is sent while waiting for the vehicle document photo
                await Respond(chatId, "Please upload a photo of your vehicle registration document 🚗", session, cancellationToken);
            }
        }

        var userText = update.Message.Text?.ToLower();

        // Confirm extracted data with user
        if (session.Stage == ConversationStage.ConfirmingData)
        {
            if (userText == "yes")
            {
                session.Stage = ConversationStage.PriceQuotation;
                await Respond(chatId, "quote_price", session, cancellationToken);
                session.Stage = ConversationStage.WaitingForPriceAgreement;
            }
            else if (userText == "no")
            {
                session.Stage = ConversationStage.WaitingForPassport;
                session.PassportPhotoFileId = null;
                session.VehiclePhotoFileId = null;
                session.ExtractedData?.Clear();
                await Respond(chatId, "incorrect_data_retry", session, cancellationToken);
            }
            else
            {
                await Respond(chatId, "confirm_data_invalid", session, cancellationToken);
            }
            return;
        }

        // Handle price agreement stage
        if (session.Stage == ConversationStage.WaitingForPriceAgreement)
        {
            if (userText == "yes")
            {
                session.Stage = ConversationStage.GeneratingPolicy;
                await Respond(chatId, "generating_policy", session, cancellationToken);

                var policy = await _policyGenerator.GenerateDummyPolicy(session.ExtractedData);
                await _botClient.SendTextMessageAsync(chatId, policy, cancellationToken: cancellationToken);
                
                session.Stage = ConversationStage.Completed;
                await Respond(chatId, "policy_sent", session, cancellationToken);
            }
            else if (userText == "no")
            {
                session.Stage = ConversationStage.PriceRejected;
                await Respond(chatId, "price_rejected", session, cancellationToken);
                session.Stage = ConversationStage.WaitingForPriceAgreement;
            }
            else
            {
                await Respond(chatId, "quote_invalid_response", session, cancellationToken);
            }
            return;
        }

        // Handle completed stage and restart prompt
        if (session.Stage == ConversationStage.Completed)
        {
            await Respond(chatId, "completed_restart_prompt", session, cancellationToken);
        }
    }

    private async Task Respond(long chatId, string message, UserSession session, CancellationToken cancellationToken)
    {
        var prompt = $"Stage: {session.Stage}\nUserData: {string.Join(", ", session.ExtractedData ?? new Dictionary<string, string>())}\nPrompt: {message}";
        var response = await _aiService.GenerateReply(prompt, session);
        await _botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
    }

    // Handle errors during bot operation
    private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
    }
}
