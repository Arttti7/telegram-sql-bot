#  Telegram SQL Bot

AI-бот для генерации SQL-запросов из естественного языка на C# с интеграцией Ollama.

# Возможности

- ✅ Преобразует текстовые запросы в SQL-код
- ✅ Работает с локальным AI (Ollama) — без облачных API, без интернета
- ✅ Поддержка MS SQL синтаксиса (SELECT, JOIN, COUNT, подзапросы)
- ✅ Бесплатный и автономный
- ✅ Обработка команд `/start` и `/help`

# Технологии

- **Язык:** C# / .NET 10
- **Библиотеки:** Telegram.Bot, Microsoft.Extensions.Configuration, System.Net.Http.Json
- **AI:** Ollama (qwen2.5-coder:7b)
- **Контроль версий:** Git + GitHub

# Установка и запуск

### 1. Установи Ollama
Скачай с [ollama.com](https://ollama.com/) и установи.

### 2. Скачай модель
```bash
ollama pull qwen2.5-coder:7b
