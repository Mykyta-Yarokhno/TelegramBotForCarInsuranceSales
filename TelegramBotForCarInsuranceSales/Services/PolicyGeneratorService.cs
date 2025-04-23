using Betalgo.Ranul.OpenAI;
using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.Managers;
using Betalgo.Ranul.OpenAI.ObjectModels;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PolicyGeneratorService
{
    private readonly IOpenAIService _client;

    public PolicyGeneratorService()
    {
        // Initialize OpenAI service with API key for car insurance policy generation
        _client = new OpenAIService(new OpenAIOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        });
    }

    // Generate a dummy car insurance policy using extracted user data
    public async Task<string> GenerateDummyPolicy(Dictionary<string, string> extractedData)
    {
        var policyInfo = string.Join("\n", extractedData.Select(kv => $"{kv.Key}: {kv.Value}"));

        var prompt = $"""
        Generate a simple but formal car insurance policy for the following user:
        {policyInfo}

        Fixed price: $100.
        Include a unique policy number and today's date.
        Use a professional tone.
        """;

        try
        {
            var result = await _client.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Model = Models.Gpt_3_5_Turbo,
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a helpful assistant generating car insurance documents."),
                    ChatMessage.FromUser(prompt)
                }
            });

            if (result.Successful)
                return result.Choices.First().Message.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenAI error: {ex.Message}");
        }

        return GenerateFallbackPolicy(extractedData);
    }

    // Fallback method to generate a default car insurance policy
    private string GenerateFallbackPolicy(Dictionary<string, string> extractedData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Car Insurance Policy ===");
        sb.AppendLine($"Policy Number: POL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");
        sb.AppendLine($"Date Issued: {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine("Insured Person Details:");

        foreach (var item in extractedData)
        {
            sb.AppendLine($"{item.Key}: {item.Value}");
        }

        sb.AppendLine("\nCoverage: Full coverage of the insured vehicle.");
        sb.AppendLine("Price: $100");
        sb.AppendLine("============================");

        return sb.ToString();
    }
}
