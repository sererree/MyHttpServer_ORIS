using System;
using System.Threading.Tasks;
using MyHttpServer;

class Program
{
    static async Task Main(string[] args)
    {
        HttpServer server = new HttpServer();

        
        Task serverTask = server.StartAsync();

        
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