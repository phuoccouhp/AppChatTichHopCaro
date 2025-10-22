using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace CaroNetClient
{
    public static class Net
    {
        static TcpClient _cli;
        static StreamReader _rd;
        static StreamWriter _wr;
        public static event Action<string> OnLine;

        public static bool Connect(string host, int port)
        {
            try
            {
                _cli = new TcpClient();
                _cli.Connect(host, port);
                var ns = _cli.GetStream();
                _rd = new StreamReader(ns);
                _wr = new StreamWriter(ns) { AutoFlush = true };
                new Thread(ReadLoop) { IsBackground = true }.Start();
                return true;
            }
            catch { return false; }
        }

        static void ReadLoop()
        {
            try
            {
                string line;
                while ((line = _rd.ReadLine()) != null)
                    OnLine?.Invoke(line);
            }
            catch { /* disconnected */ }
        }

        public static void Send(string line)
        {
            try { _wr.WriteLine(line); }
            catch { /* ignore */ }
        }
    }
}
