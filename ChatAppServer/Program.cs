using System;
using System.Threading.Tasks;

namespace ChatAppServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 9000;
            Server server = new Server(port);

            Logger.Success($"Server đã khởi động và lắng nghe tại port {port}.");

            await server.StartAsync();
        }
    }
}