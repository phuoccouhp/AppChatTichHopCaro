using System;
using System.Threading.Tasks;

namespace ChatAppServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Chọn 1 port bất kỳ
            int port = 9000;

            Server server = new Server(port);

            // Chạy server vĩnh viễn
            await server.StartAsync();
        }
    }
}