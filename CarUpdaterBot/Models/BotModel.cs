using Telegram.Bot;

namespace CarUpdaterBot.Models
{
    public static class BotModel
    {
        public static ITelegramBotClient BotClient
        {
            get { return new TelegramBotClient("806565363:AAH7QTQlJo3o0eE7zeLaNYCjZyvkL4JELsw"); }
        }

        public static string ApiUrl { get; set; } = "https://carupdatebot...";
    }
}