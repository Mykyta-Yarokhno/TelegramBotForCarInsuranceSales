using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotForCarInsuranceSales.Models
{
    public enum ConversationStage
    {
        None,
        WaitingForPassport,
        WaitingForVehicleDoc,
        ConfirmingData,
        PriceQuotation,
        WaitingForPriceAgreement,
        PriceRejected,
        GeneratingPolicy,
        Completed
    }
}
