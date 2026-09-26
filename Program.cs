using System;
using System.Threading.Tasks;
using MyHttpServer;

class Program
{
    static async Task Main(string[] args)
    {
        HttpServer server = new HttpServer();

        // Запускаем сервер в фоновой задаче, чтобы не блокировать главный поток консоли
        Task serverTask = server.StartAsync();

        // Ожидаем команду остановки из консоли (например, "stop" или "exit")
        string command;
        while (true)
        {
            command = Console.ReadLine();
            if (command == "stop" || command == "exit")
            {
                server.Stop();
                break;
            }
        }

        await serverTask;
    }
}