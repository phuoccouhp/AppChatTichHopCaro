using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CaroServer
{
    class ClientCtx
    {
        public string User;
        public TcpClient Cli;
        public StreamReader Rd;
        public StreamWriter Wr;
        public string Opponent; // username đối thủ
        public string Symbol;   // "X" hoặc "O"
    }

    class Program
    {
        static TcpListener listener;
        static ConcurrentDictionary<string, ClientCtx> byUser = new ConcurrentDictionary<string, ClientCtx>();
        static ConcurrentDictionary<TcpClient, ClientCtx> byClient = new ConcurrentDictionary<TcpClient, ClientCtx>();

        static void Main(string[] args)
        {
            int port = 5555;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server listening on port " + port);

            while (true)
            {
                var cli = listener.AcceptTcpClient();
                new Thread(() => Handle(cli)) { IsBackground = true }.Start();
            }
        }

        static void Handle(TcpClient cli)
        {
            try
            {
                var ns = cli.GetStream();
                var rd = new StreamReader(ns);
                var wr = new StreamWriter(ns) { AutoFlush = true };
                var ctx = new ClientCtx { Cli = cli, Rd = rd, Wr = wr };
                byClient[cli] = ctx;

                string line;
                while ((line = rd.ReadLine()) != null)
                {
                    string cmd = line, rest = "";
                    int sp = line.IndexOf(' ');
                    if (sp >= 0) { cmd = line.Substring(0, sp); rest = line.Substring(sp + 1); }

                    if (cmd == "LOGIN")
                    {
                        string user = rest.Trim();
                        if (string.IsNullOrWhiteSpace(user) || byUser.ContainsKey(user))
                        {
                            wr.WriteLine("ERROR username_invalid_or_taken");
                        }
                        else
                        {
                            ctx.User = user;
                            byUser[user] = ctx;
                            wr.WriteLine("LOGIN_OK");
                            Console.WriteLine("LOGIN " + user);
                        }
                    }
                    else if (cmd == "CHAT")
                    {
                        // CHAT <to> <text...>
                        var p = rest.Split(new[] { ' ' }, 2, StringSplitOptions.None);
                        var to = p[0];
                        var text = p.Length > 1 ? p[1] : "";
                        ClientCtx tctx;
                        if (byUser.TryGetValue(to, out tctx))
                            tctx.Wr.WriteLine("CHAT " + ctx.User + " " + text);
                        else
                            wr.WriteLine("ERROR user_not_found");
                    }
                    else if (cmd == "INVITE")
                    {
                        string target = rest.Trim();
                        ClientCtx tctx;
                        if (!byUser.TryGetValue(target, out tctx))
                        {
                            wr.WriteLine("ERROR user_not_found");
                        }
                        else
                        {
                            // ghép cặp
                            ctx.Opponent = target; ctx.Symbol = "X";
                            tctx.Opponent = ctx.User; tctx.Symbol = "O";
                            ctx.Wr.WriteLine("INVITE_OK " + target + " " + ctx.Symbol);
                            tctx.Wr.WriteLine("INVITE_OK " + ctx.User + " " + tctx.Symbol);
                            Console.WriteLine("PAIR " + ctx.User + " vs " + target);
                        }
                    }
                    else if (cmd == "MOVE")
                    {
                        // MOVE r c
                        var p = rest.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (p.Length >= 2)
                        {
                            int r, c;
                            if (int.TryParse(p[0], out r) && int.TryParse(p[1], out c))
                            {
                                if (ctx.Opponent != null)
                                {
                                    ClientCtx opp;
                                    if (byUser.TryGetValue(ctx.Opponent, out opp))
                                        opp.Wr.WriteLine("OPP_MOVE " + r + " " + c + " " + ctx.Symbol);
                                }
                            }
                        }
                    }
                    else if (cmd == "RESULT")
                    {
                        // RESULT WIN/LOSS/DRAW (đơn giản: relay cho đối thủ nếu cần)
                        if (ctx.Opponent != null)
                        {
                            ClientCtx opp;
                            if (byUser.TryGetValue(ctx.Opponent, out opp))
                                opp.Wr.WriteLine("RESULT " + rest.Trim());
                        }
                    }
                    else if (cmd == "PING")
                    {
                        wr.WriteLine("PONG");
                    }
                    else
                    {
                        wr.WriteLine("ERROR unknown_command");
                    }
                }
            }
            catch { /* ignore */ }
            finally
            {
                ClientCtx ctx;
                if (byClient.TryRemove(cli, out ctx))
                {
                    if (ctx != null && ctx.User != null)
                    {
                        ClientCtx removed;
                        byUser.TryRemove(ctx.User, out removed);
                    }
                    Console.WriteLine("DISCONNECT " + (ctx != null ? ctx.User : ""));
                }
                try { cli.Close(); } catch { }
            }
        }
    }
}
