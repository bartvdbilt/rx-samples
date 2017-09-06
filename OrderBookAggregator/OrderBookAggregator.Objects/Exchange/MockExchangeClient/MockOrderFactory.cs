using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class MockOrderFactory
    {
        int _nextID;

        static Random _rnd = new Random();
        static readonly object syncLock = new object();
        
        public MockOrderFactory(int exchangeID, IScheduler scheduler)
        {
            _exchangeID = exchangeID;
            _scheduler = scheduler;
        }

        private readonly int _exchangeID;
        private IScheduler _scheduler;

        int RndNext(int maxValue)
        {
            lock (syncLock)
            { // synchronize
                return _rnd.Next(maxValue);
            }
        }

        double RndNextDouble()
        {
            lock (syncLock)
            { // synchronize
                return _rnd.NextDouble();
            }
        }

        IConnectableObservable<ExchangeOrder> randomLimitOrders;
        public IConnectableObservable<ExchangeOrder> RandomLimitOrders
        {
            get
            {
                if (randomLimitOrders == null)
                {
                    var period = TimeSpan.FromSeconds(3);

                    randomLimitOrders = Observable.Create<ExchangeOrder>(obs =>
                    {
                        obs.OnNext(CreateRandomLimitOrder(Interlocked.Increment(ref _nextID), 100, 10));
                        return Disposable.Empty;
                    })
                    .Concat(Observable.Empty<ExchangeOrder>().Delay(period, _scheduler))
                    .Repeat()
                    .Timeout(TimeSpan.FromSeconds(5), _scheduler)
                    .Retry()
                    .Publish();

                }
                return randomLimitOrders;
            }
        }

        ExchangeOrder CreateRandomLimitOrder(int id, decimal basePrice, decimal baseVolume)
        {
            var otype = OrderType.Limit;
            var oside = (RndNextDouble() < 0.5) ? OrderSide.Buy : OrderSide.Sell;

            decimal priceDeltaDirection = (RndNextDouble() < 0.5) ? -1 : 1;
            decimal priceDeltaPercentage = RndNext(10) / 100.0m;
            decimal priceDelta = priceDeltaDirection * priceDeltaPercentage * basePrice;

            decimal price = basePrice + priceDelta;

            var size = (1 + (decimal)RndNextDouble()) * baseVolume;

            return new ExchangeOrder(id, _exchangeID, oside, otype, price, size);

        }

        public ExchangeOrder CreateOrder(OrderSide orderSide, OrderType orderType, decimal price, decimal size)
        {
            return new ExchangeOrder(Interlocked.Increment(ref _nextID), _exchangeID, orderSide, orderType, price, size);
        }
    }
}
