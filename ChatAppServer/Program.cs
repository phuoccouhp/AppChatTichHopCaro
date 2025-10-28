using System;
using System.Threading.Tasks;

// **ĐÚNG NAMESPACE CHO SERVER**
namespace ChatAppServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 9000;
            Server server = new Server(port);

            // Log khi khởi động
            Logger.Success($"Server đã khởi động và lắng nghe tại port {port}.");

            await server.StartAsync(); // Chạy Server
        }
    }
}