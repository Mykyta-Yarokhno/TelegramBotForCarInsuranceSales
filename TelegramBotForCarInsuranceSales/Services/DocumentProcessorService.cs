using RestSharp;
using System;
using System.Net.Http.Headers;
using System.Text.Json;
using Telegram.Bot;

public class DocumentProcessorService
{
    private readonly TelegramBotClient _botClient;

    public DocumentProcessorService()
    {
        _botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"));
    }

    // Extracts data from the provided passport and vehicle file IDs
    public async Task<Dictionary<string, string>> ExtractData(string passportFileId, string vehicleFileId)
    {
        var passportFilePath = await DownloadTelegramFile(passportFileId);
        var vehicleFilePath = await DownloadTelegramFile(vehicleFileId);

        // Mock extracted data (to be replaced with actual extraction logic)
        var mockData = new Dictionary<string, string>
        {
            { "Full Name", "Sarah Martin" },
            { "Passport Number", "P123456AA" },
            { "Nationality", "Canadian" },
            { "Date of Birth", "01 Aug 1990" },
            { "Vehicle Make", "Toyota" },
            { "Vehicle Model", "Corolla" },
            { "Vehicle Year", "2018" },
            { "Registration Number", "BI7378HK" }
        };

        return mockData;
    }

    // Downloads a file from Telegram given its file ID
    private async Task<string> DownloadTelegramFile(string fileId)
    {
        var file = await _botClient.GetFileAsync(fileId); 
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg"); 

        using (var saveStream = File.OpenWrite(filePath)) 
        {
            await _botClient.DownloadFileAsync(file.FilePath, saveStream);
        }

        return filePath;
    }
}