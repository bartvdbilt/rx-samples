using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;
using vtortola.WebSockets.Deflate;

namespace ChatServer
{
    class ReactiveWebSocketServer
    {
        private CancellationTokenSource cancellation;
        private WebSocketListener server;

        public ReactiveWebSocketServer()
        {
            cancellation = new CancellationTokenSource();
        }

        public void startServer()
        {
            server.Start();
        }

        public void subscribe()
        {
            var chatSessionObserver = new ChatSessionsObserver(new ChatRoomManager());

            Observable.FromAsync(server.AcceptWebSocketAsync)
                      .Select(ws => new ChatSession(ws)
                      {
                          In = Observable.FromAsync<dynamic>(ws.ReadDynamicAsync)
                                         .DoWhile(() => ws.IsConnected)
                                         .Where(msg => msg != null)
                                         .Throttle(TimeSpan.FromSeconds(0.5)),

                          Out = Observer.Create<dynamic>(ws.WriteDynamic)
                      })
                      .DoWhile(() => server.IsStarted && !cancellation.IsCancellationRequested)
                      .Subscribe(chatSessionObserver);
        }

        public void unSubscribe()
        {
            cancellation.Cancel();
        }

        public void createReactiveWebSocketServer(IPAddress ip, int port)
        {
            var endpoint = new IPEndPoint(ip, port);
            server = new WebSocketListener(endpoint);
            setRFC6455ServerStandards();
        }

        private void setRFC6455ServerStandards()
        {
            var rfc6455 = new vtortola.WebSockets.Rfc6455.WebSocketFactoryRfc6455(server);
            rfc6455.MessageExtensions.RegisterExtension(new WebSocketDeflateExtension());
            server.Standards.RegisterStandard(rfc6455);
        }
    }
}
