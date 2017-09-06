using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using vtortola.WebSockets;
using System.Reactive.Subjects;
using vtortola.WebSockets.Deflate;

namespace ChatServer
{
    class Program
    {
        static void Main(String[] args)
        {
            IPAddress ip = IPAddress.Any;
            int port = 81;

            ReactiveWebSocketServer server = new ReactiveWebSocketServer();
            server.createReactiveWebSocketServer(ip, port);
            server.startServer();

            Log("Rx Chat Server started at " + ip + ":" + port);

            server.subscribe();

            Console.ReadKey(true);
            Log("Server stoping");
            server.unSubscribe();
            Console.ReadKey(true);
        }

        static void Log(String s)
        {
            Console.WriteLine(s);
        }
    }
}
