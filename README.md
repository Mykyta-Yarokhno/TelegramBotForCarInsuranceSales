# 🤖 Telegram Bot for Car Insurance Sales

This is a C# .NET-based Telegram bot that guides users through purchasing car insurance via a conversational flow powered by OpenAI and document scanning.

---

## 🚀 Features

- Step-by-step onboarding for insurance purchase
- Passport and vehicle document upload
- AI-powered data extraction and validation
- Fixed price offering and policy generation
- Context-aware replies via OpenAI GPT

---

## 🛠 Setup Instructions

###  Dependencies
- .NET 8 SDK
- Telegram Bot v18
- Betalgo.Ranul.OpenAI

###  Installation

```bash
git clone https://github.com/Mykyta-Yarokhno/TelegramBotForCarInsuranceSales.git
cd TelegramBotForCarInsuranceSales
dotnet restore
```

1. Replace your OpenAI key in `AIConversationService.cs`
2. Replace your Telegram token in `BotService.cs`
3. Run the bot:
```bash
dotnet run
```

---

## 🔄 Bot Workflow

1. **Start Command** — `/start` begins the session.
2. **Passport Upload** — User sends a photo of their passport.
3. **Vehicle Document Upload** — User sends their vehicle registration.
4. **Data Confirmation** — Bot extracts data and asks: `Is everything correct? (yes/no)`
5. **Price Agreement** — Fixed price of `100 USD`. User must reply `yes` to proceed.
6. **Policy Generation** — Final document is generated and sent.

---

## 💬 Sample Interaction

```text
User: /start
Bot: Welcome! Please upload a photo of your passport 📷

User: [passport.jpg]
Bot: Thanks! Now please upload your vehicle registration document 🚗

User: [vehicle.jpg]
Bot: Here's the extracted data:
- Full Name: Sarah Martin
- Passport Number: P123456AA
- Nationality: Canadian
- Date of Birth: 01 Aug 1990
- Vehicle Make: Toyota
- Vehicle Model: Corolla
- Vehicle Year: 2018
- Registration Number: BI7378HK

Is everything correct? Please reply 'yes' or 'no'.

User: yes
Bot: The fixed price is 100 USD. Do you agree? Reply 'yes' or 'no'.

User: yes
Bot: Generating your insurance policy... 🧾
Bot: ✅ Done! Your policy has been sent.
```

---

