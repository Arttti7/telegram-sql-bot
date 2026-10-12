using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgSqlBot
{
    public class BotHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly AIService _aiService;

        public BotHandler(TelegramBotClient botClient, AIService aiService)
        {
            _botClient = botClient;
            _aiService = aiService;
        }

        public async Task HandleMessageAsync(Message message)
        {
            if (message.Type != MessageType.Text)
                return;

            var text = message.Text.Trim();

            // Обработка команд
            if (text.StartsWith("/"))
            {
                await HandleCommandAsync(message, text);
                return;
            }

            // Обработка обычных сообщений (генерация SQL)
            try
            {
                await _botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "⏳ Генерирую SQL-запрос...",
                    cancellationToken: CancellationToken.None
                );

                var sqlQuery = await _aiService.GenerateSqlAsync(text);

                await _botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"✅ SQL-запрос:\n\n`{sqlQuery}`",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"❌ Ошибка: {ex.Message}",
                    cancellationToken: CancellationToken.None
                );
            }
        }

        private async Task HandleCommandAsync(Message message, string command)
        {
            switch (command.ToLower())
            {
                case "/start":
                    await _botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "👋 Привет! Я SQL-ассистент.\n\nОтправь мне запрос на естественном языке, и я превращу его в SQL-запрос.\n\nПример: *Покажи всех студентов с оценкой выше 7*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: CancellationToken.None
                    );
                    break;

                case "/help":
                    await _botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "📖 Доступные команды:\n/start - Начать работу\n/help - Справка\n\nПросто напиши запрос, например:\n• Покажи студентов с оценкой выше 8\n• Сколько курсов доступно?\n• Найди студентов, записанных на Математику",
                        cancellationToken: CancellationToken.None
                    );
                    break;

                default:
                    await _botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "❓ Неизвестная команда. Используй /help для списка команд.",
                        cancellationToken: CancellationToken.None
                    );
                    break;
            }
        }
    }
}