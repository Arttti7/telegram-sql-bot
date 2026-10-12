using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace TgSqlBot
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public AIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _model = configuration["OpenRouterModel"] ?? "qwen2.5-coder:7b";
        }

        public async Task<string> GenerateSqlAsync(string userRequest)
        {
            var systemPrompt = @"Ты — генератор SQL-запросов. Твоя единственная задача — преобразовать запрос пользователя в SQL-код.

СТРОГИЕ ПРАВИЛА:
1. Отвечай ТОЛЬКО SQL-кодом, без объяснений, без комментариев, без markdown.
2. Используй ТОЛЬКО эти таблицы:
   - Students(Id INT, Name NVARCHAR(100), Grade INT)
   - Courses(Id INT, Title NVARCHAR(200))
   - Enrollments(StudentId INT, CourseId INT)
3. Если запрос непонятен — верни: SELECT 1 AS Error -- Неясный запрос

ПРИМЕРЫ:
Пользователь: Покажи всех студентов с оценкой выше 7
Ты: SELECT Name, Grade FROM Students WHERE Grade > 7

Пользователь: Сколько студентов имеют оценку выше 8?
Ты: SELECT COUNT(*) AS StudentCount FROM Students WHERE Grade > 8

Теперь обработай запрос пользователя:";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userRequest }
                },
                stream = false,
                options = new
                {
                    temperature = 0.1,
                    num_predict = 150
                }
            };

            try
            {
                Console.WriteLine($"🔌 Подключение к Ollama: http://localhost:11434/api/chat");
                Console.WriteLine($"📝 Модель: {_model}");
                Console.WriteLine($"💬 Запрос: {userRequest}");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(60);

                var response = await client.PostAsJsonAsync(
                    "http://localhost:11434/api/chat",
                    requestBody
                );

                Console.WriteLine($"✅ Статус ответа: {(int)response.StatusCode} {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📄 Ответ от Ollama: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                var sql = result?.Message?.Content?.Trim();
                
                if (sql != null)
                {
                    sql = sql.Replace("```sql", "").Replace("```", "").Trim();
                }
                
                return sql ?? "Не удалось сгенерировать SQL";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Ошибка подключения к Ollama: {ex.Message}");
                Console.WriteLine($"💡 Убедись, что Ollama запущен (иконка ламы в трее)");
                return $"Ошибка подключения к Ollama: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Общая ошибка: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                return $"Ошибка: {ex.Message}";
            }
        }
    }

    public class OllamaResponse
    {
        public OllamaMessage Message { get; set; }
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Done { get; set; }
    }

    public class OllamaMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}