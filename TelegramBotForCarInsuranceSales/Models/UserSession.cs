using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotForCarInsuranceSales.Models
{
    public class UserSession
    {
        public ConversationStage Stage { get; set; } = ConversationStage.None;
        public string? PassportPhotoFileId { get; set; }
        public string? VehiclePhotoFileId { get; set; }
        public Dictionary<string, string>? ExtractedData { get; set; }
        public bool IsConfirmed { get; set; }

        // Reset session to initial state
        public void Reset()
        {
            PassportPhotoFileId = null;
            VehiclePhotoFileId = null;
            Stage = ConversationStage.None;
            ExtractedData = null;
        }
    }
}
