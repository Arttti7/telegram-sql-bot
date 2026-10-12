using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TgSqlBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(" Запуск Telegram SQL-бота...");

            // Загрузка конфигурации
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var botToken = configuration["TelegramBotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("❌ Ошибка: Токен бота не найден в appsettings.json");
                return;
            }

            // Инициализация сервисов
            var botClient = new TelegramBotClient(botToken);
            var aiService = new AIService(configuration);
            var botHandler = new BotHandler(botClient, aiService);

            // Настройка обработки сообщений
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: async (bot, update, cancellationToken) =>
                {
                    if (update.Message != null)
                    {
                        Console.WriteLine($"📩 Получено сообщение от {update.Message.Chat.Id}: {update.Message.Text}");
                        await botHandler.HandleMessageAsync(update.Message);
                    }
                },
                pollingErrorHandler: async (bot, exception, cancellationToken) =>
                {
                    Console.WriteLine($"⚠️ Ошибка polling: {exception.Message}");
                    await Task.CompletedTask;
                },
                receiverOptions: receiverOptions
            );

            Console.WriteLine("✅ Бот запущен! Нажмите Ctrl+C для остановки.");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
