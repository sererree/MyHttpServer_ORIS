using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public class HttpServer
    {
        private HttpListener _listener;
        private string _urlPrefix;
        private bool _isRunning;

        public HttpServer()
        {
            _listener = new HttpListener();

            // Проверка на существование файла settings.json
            if (!File.Exists("settings.json"))
            {
                Console.WriteLine("Ошибка: файл settings.json не найден!");
                return;
            }

            string settingsJson = File.ReadAllText("settings.json");
            Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

            _urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/{setting.Server.Path}";
            _listener.Prefixes.Add(_urlPrefix);
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
                _isRunning = true;
                Console.WriteLine("Сервер запущен и слушает: " + _urlPrefix);

                // Бесконечный цикл постоянного прослушивания запросов
                while (_isRunning)
                {
                    try
                    {
                        // Получаем контекст асинхронно
                        var context = await _listener.GetContextAsync();

                        // Обрабатываем запрос в фоновом режиме, чтобы не блокировать цикл
                        _ = ProcessRequestAsync(context);
                    }
                    catch (HttpListenerException)
                      when (!_isRunning)
                    {
                        // Исключение при штатной остановке лисенера — это нормально
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при работе сервера: {ex.Message}");
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;

            // Проверка на существование файла search-engine.html (согласно заданию)
            string htmlFileName = "search-engine.html";
            if (!File.Exists(htmlFileName))
            {
                Console.WriteLine($"Ошибка: файл {htmlFileName} не найден!");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                byte[] notFoundBuffer = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1>");
                response.ContentLength64 = notFoundBuffer.Length;
                await response.OutputStream.WriteAsync(notFoundBuffer);
                await response.OutputStream.FlushAsync();
                return;
            }

            // Читаем текст HTML-страницы поиска
            string htmlFileText = File.ReadAllText(htmlFileName);
            byte[] buffer = Encoding.UTF8.GetBytes(htmlFileText);

            // Отправляем данные клиенту
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();

            Console.WriteLine("Запрос обработан");
        }

        public void Stop()
        {
            _isRunning = false;
            if (_listener.IsListening)
            {
                _listener.Stop();
                Console.WriteLine("Сервер завершил работу");
            }
        }
    }
}