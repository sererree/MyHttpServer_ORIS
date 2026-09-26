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

                
                while (_isRunning)
                {
                    try
                    {
                        
                        var context = await _listener.GetContextAsync();

                        
                        _ = ProcessRequestAsync(context);
                    }
                    catch (HttpListenerException)
                      when (!_isRunning)
                    {
                        
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
    string requestPath = context.Request.Url.AbsolutePath.TrimStart('/');

    // Если запрашивают корень, отдаем search.html
    if (string.IsNullOrEmpty(requestPath) || requestPath == "connection")
    {
        requestPath = "search.html";
    }

    // Проверяем существование файла
    if (!File.Exists(requestPath))
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        byte[] notFoundBuffer = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1>");
        response.ContentLength64 = notFoundBuffer.Length;
        await response.OutputStream.WriteAsync(notFoundBuffer);
        await response.OutputStream.FlushAsync();
        return;
    }

    // Устанавливаем правильный Content-Type в зависимости от расширения
    if (requestPath.EndsWith(".css"))
    {
        response.ContentType = "text/css; charset=utf-8";
    }
    else if (requestPath.EndsWith(".html"))
    {
        response.ContentType = "text/html; charset=utf-8";
    }
    else if (requestPath.EndsWith(".png") || requestPath.EndsWith(".svg"))
    {
        response.ContentType = requestPath.EndsWith(".svg") ? "image/svg+xml" : "image/png";
    }

    byte[] buffer = await File.ReadAllBytesAsync(requestPath);
    response.ContentLength64 = buffer.Length;
    
    using Stream output = response.OutputStream;
    await output.WriteAsync(buffer);
    await output.FlushAsync();

    Console.WriteLine($"Запрос обработан: {requestPath}");
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