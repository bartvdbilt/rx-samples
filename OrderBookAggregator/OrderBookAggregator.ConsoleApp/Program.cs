using OrderBookAggregator.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAggregatedOrderBookStreamOnMocks();
        }

        static void TestAggregatedOrderBookStreamOnMocks()
        {
            IExchangeClient exchangeA = new MockClient(0);
            IExchangeClient exchangeB = new MockClient(1);
            IExchangeClient exchangeC = new MockClient(2);

            IObservable<IOrderBook> mergedOrderBookStream = Observable.Merge(exchangeA.OrderBookStream,
                                                                             exchangeB.OrderBookStream,
                                                                             exchangeC.OrderBookStream);

            IObservable<AggregatedOrderBook> aggregatedOrderBookStream = mergedOrderBookStream.Scan(
                                                                        new AggregatedOrderBook(),
                                                                        (aob, orderBook) => aob.InsertBook(orderBook));

            using (var consoleSubscription = aggregatedOrderBookStream.Subscribe(Console.WriteLine))
            {
                Console.WriteLine("press any key to stop");
                var exchanges = new IExchangeClient[3]{exchangeA, exchangeB, exchangeC};
                exchanges.ToList().ForEach(ex => { ex.Connect(); ((MockClient)ex).StartRandomOrders(); });                
                Console.ReadKey();
                exchanges.ToList().ForEach(ex => { ex.Disconnect(); ((MockClient)ex).StopRandomOrders(); });

            }
            
            Console.WriteLine("stopped. Press any key to close window");
            Console.ReadKey();
        }

        static void TestMockExchangeOrderBook()
        {
            var mockExchange = new MockClient(1);

            var orderStream = mockExchange.OrderStream;

            var orderBookStream = mockExchange.OrderBookStream;

            using(var orderStreamSub = orderStream.Subscribe(Console.WriteLine))
            using (var orderBookSteamSub = orderBookStream.Subscribe(Console.WriteLine))
            {
                Console.WriteLine("Press any key to stop");
                mockExchange.Connect();
                mockExchange.StartRandomOrders();
                Console.ReadKey();
                mockExchange.StopRandomOrders();
                mockExchange.Disconnect();
            }

            Console.WriteLine("Press any key to close window");
            Console.ReadKey();
        }

        static void TestOrderFactory()
        {
            var exchangeA = new MockClient(1);
            var exchangeB = new MockClient(2);

            using( var aSub = exchangeA.OrderStream.Subscribe( o=> Console.WriteLine("A: {0}", o)))
            using (var bSub = exchangeB.OrderStream.Subscribe(o => Console.WriteLine("B: {0}", o)))
            {
                Console.WriteLine("Press any key to stop");
                exchangeA.Connect(); exchangeA.StartRandomOrders();
                exchangeB.Connect(); exchangeB.StartRandomOrders();
                Console.ReadKey();
                exchangeA.Disconnect(); exchangeA.StopRandomOrders();
                exchangeB.Disconnect(); exchangeB.StopRandomOrders();
            }
        }

    }
}
