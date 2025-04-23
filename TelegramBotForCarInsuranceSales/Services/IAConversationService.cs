using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.Managers;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Betalgo.Ranul.OpenAI.ObjectModels;
using Betalgo.Ranul.OpenAI;
using TelegramBotForCarInsuranceSales.Models;

public class AIConversationService
{
    private readonly IOpenAIService _openAiService;

    public AIConversationService()
    {
        // Initialize OpenAI service with API key

        _openAiService = new OpenAIService(new OpenAIOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        });
    }

    // Generate a reply to the user based on the current stage and user message
    public async Task<string?> GenerateReply(string userMessage, UserSession? session)
    {
        try
        {
            var prompt = BuildPrompt(userMessage, session);

            var result = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Model = Models.Gpt_3_5_Turbo,
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a friendly car insurance assistant. Your goal is to guide the user through purchasing car insurance by collecting documents, confirming data, offering a price, and generating a policy. Follow the current stage in the conversation and respond accordingly."),
                    ChatMessage.FromUser(prompt)
                }
            });

            if (result.Successful && result.Choices.Any())
            {
                return result.Choices.First().Message.Content;
            }
            else
            {
                return GetDefaultResponse(session);
            }
        }
        catch
        {
            return GetDefaultResponse(session);
        }
    }

    // Returns a default response depending on the current stage
    private string GetDefaultResponse(UserSession? session)
    {
        var stage = session?.Stage ?? ConversationStage.None;

        return stage switch
        {
            ConversationStage.WaitingForPassport =>
                "Please upload a photo of your passport 📷",

            ConversationStage.WaitingForVehicleDoc =>
                "Thanks! Now please upload a photo of your vehicle registration document 🚗",

            ConversationStage.ConfirmingData =>
                BuildConfirmingMessage(session),

            ConversationStage.PriceQuotation =>
                "Thank you for confirming your information. The fixed price for your car insurance is 100 USD. Do you agree to proceed with this price? Please reply with 'yes' or 'no'. If not, please note that the price is fixed and cannot be changed.",

            ConversationStage.WaitingForPriceAgreement =>
                "Please let us know if you agree with the proposed price of 100 USD. If you reply 'yes', we’ll proceed to finalize your insurance. If you reply 'no' or have concerns, we understand — but please note that 100 USD is the only available price and cannot be changed.",

            ConversationStage.PriceRejected =>
                "We understand that the price might be a concern. Unfortunately, the fixed price for the insurance is 100 USD and cannot be changed. If you agree to proceed with this price, please reply 'yes'. Otherwise, reply 'no'.",

            ConversationStage.GeneratingPolicy =>
                "Generating your insurance policy. Please wait a few seconds... 🧾",

            ConversationStage.Completed =>
                "Thank you for using our service! Your insurance policy has been generated. You can now close the chat or start over by typing /start.",

            ConversationStage.None =>
                "How can I help you with car insurance? Type /start to begin.",

            _ =>
                "Sorry, I couldn't generate a response. Please try again.",
        };
    }

    // Build the AI prompt based on the current conversation stage and user message
    private string BuildPrompt(string userMessage, UserSession? session)
    {
        var stage = session?.Stage ?? ConversationStage.None;

        return stage switch
        {
            ConversationStage.WaitingForPassport =>
                $"The user just started. Politely ask them to upload a photo of their passport.",

            ConversationStage.WaitingForVehicleDoc =>
                $"The user has uploaded their passport. Ask them to now upload a photo of their vehicle registration document.",

            ConversationStage.ConfirmingData =>
                    session ?.ExtractedData is { Count: > 0 }
                    ? $"The user has uploaded both required documents. These are the extracted details:\n{string.Join("\n", session.ExtractedData.Select(kv => $"{kv.Key}: {kv.Value}"))}.\n\nPlease ask the user to confirm the information by replying 'yes' or 'no'. If they reply 'no', ask them to re-upload the photos."
                    : "You are supposed to help the user confirm extracted data, but no data is available. Politely ask the user to re-upload the documents.",

            ConversationStage.PriceQuotation =>
                "The user has confirmed the extracted data. Inform them that the fixed price for the insurance is 100 USD. Ask if they agree to proceed by replying 'yes' or 'no'. If not, explain that the price is fixed and cannot be changed.",

            ConversationStage.WaitingForPriceAgreement =>
                "Wait for the user's response to the proposed price. If the user replies 'yes', thank them and proceed to the final step. If the user replies 'no' or expresses disagreement, apologize and explain that 100 USD is the only available price and cannot be changed.",

            ConversationStage.PriceRejected =>
                "The user has rejected the fixed price of 100 USD. Apologize politely and explain that the price is not negotiable. Ask if they agree to proceed by replying 'yes' or 'no'.",

            ConversationStage.GeneratingPolicy =>
                "The user agreed to the price. Inform them that their policy is being generated.",

            ConversationStage.Completed =>
                "The insurance policy has been sent. Thank the user and let them know they can start over with /start.",

            _ =>
                userMessage
        };
    }

    // Build the message to confirm extracted data from the documents
    private static string BuildConfirmingMessage(UserSession? session)
    {
        if (session?.ExtractedData is { Count: > 0 })
        {
            string preview = string.Join("\n",
                session.ExtractedData.Select(kv => $"{kv.Key}: {kv.Value}"));

            return $"Here is the extracted data from your documents:\n{preview}\n\nIs everything correct? Please reply 'yes' or 'no'.";
        }

        return "Here is the extracted data from your documents. Is everything correct? Please reply 'yes' or 'no'.";
    }
}