using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class MockClient : IExchangeClient
    {
        readonly int _exchangeID;
        MockOrderFactory _orderFactory;
        IDisposable _orderFactoryConnection;
        IDisposable _orderStreamConnection;
        IDisposable _newAndCanceledOrderStreamSubscription;
        IDisposable _orderBookStreamConnection;

        public MockClient(int exchangeID, IScheduler scheduler = null)
        {
            _exchangeID = exchangeID;

            if (scheduler == null)
                scheduler = Scheduler.Default;

            _orderFactory = new MockOrderFactory(_exchangeID, scheduler);
            _submittedOrders = new Subject<ExchangeOrder>();
            _cancelledOrders = new Subject<ExchangeOrder>();

            _orderStream = _orderFactory.RandomLimitOrders.Merge(_submittedOrders).Publish();

            _newAndCanceledOrderStream = _orderStream.Merge(_cancelledOrders).Publish();

            _orderBookStream = _newAndCanceledOrderStream.Scan(new MockOrderBook(exchangeID), (ob, o) => ob.PlaceOrder(o)).Publish();
        }

        Subject<ExchangeOrder> _submittedOrders;
        Subject<ExchangeOrder> _cancelledOrders;

        IConnectableObservable<ExchangeOrder> _newAndCanceledOrderStream;

        public int ExchangeID
        {
            get
            {
                return _exchangeID;
            }
        }

        IConnectableObservable<ExchangeOrder> _orderStream;
        public IObservable<ExchangeOrder> OrderStream
        {
            get
            {
                return _orderStream;
            }
        }

        IConnectableObservable<MockOrderBook> _orderBookStream;
        public IObservable<IOrderBook> OrderBookStream
        {
            get
            {
                return _orderBookStream;
            }
        }

        public async Task<ExchangeOrder> SubmitOrder(OrderSide oSide, OrderType oType, decimal price, decimal size)
        {
            //use factory to create an Order with the right ID
            var order = _orderFactory.CreateOrder(oSide, oType, price, size);

            //Define the Order's lifetime stream
            //IObservable<ExchangeOrder> orderLifeTimeStream = _orderBookStream.Select(ob => ob.Orders[order.ID])
            //                                                                 .TakeWhileInclusive(o => (o.Status != ExchangeOrderStatus.Closed) || (o.Status != ExchangeOrderStatus.Canceled))
            //                                                                 .LastAsync()
            //                                                                 .Do(Console.WriteLine);
            IObservable<ExchangeOrder> firstCloseOrder = _orderBookStream.Select(ob => ob.Orders[order.ID])
                .Do(Console.WriteLine)
                .Where(o => (o.Status == ExchangeOrderStatus.Closed) || (o.Status == ExchangeOrderStatus.Canceled))
                .FirstAsync().Replay();


            //pass order to a subject that is merged with the random orders
            _submittedOrders.OnNext(order);

            //wait until the order is fully filled
            var orderAtEnd = await firstCloseOrder;

            return orderAtEnd;
        }

        public async Task<ExchangeOrder> CancelOrder(ExchangeOrder order)
        {
            var canceledOrder = order.WithUpdatedStatus(ExchangeOrderStatus.Canceled);
            _cancelledOrders.OnNext(canceledOrder);
            return canceledOrder;
        }

        public void Connect()
        {
            _orderStreamConnection = _orderStream.Connect();
            _newAndCanceledOrderStreamSubscription = _newAndCanceledOrderStream.Connect();
            _orderBookStreamConnection = _orderBookStream.Connect();
        }

        public void Disconnect()
        {
            _orderStreamConnection.Dispose();
            _newAndCanceledOrderStreamSubscription.Dispose();
            _orderBookStreamConnection.Dispose();

        }

        public void StartRandomOrders()
        {
            _orderFactoryConnection = _orderFactory.RandomLimitOrders.Connect();
        }

        public void StopRandomOrders()
        {
            _orderFactoryConnection.Dispose();
        }
    }
}
