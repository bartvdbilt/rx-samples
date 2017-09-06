using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Reactive.Testing;
using OrderBookAggregator.Objects;
using System.Collections.Generic;

namespace OrderBookAggregator.Tests
{
    [TestClass]
    public class ExchangeClientTests
    {
        /// <summary>
        /// Test Submitted orders are added to the order and orderbook streams
        /// </summary>
        [TestMethod]
        public void TestSubmittedOrdersAreAddedToStreams()
        {
            var testScheduler = new TestScheduler();

            var mockExchangeClient = new MockClient(1, testScheduler);

            var orderStream = mockExchangeClient.OrderStream;
            var orderBookStream = mockExchangeClient.OrderBookStream;

            var sourceOrderList = new List<ExchangeOrder>();

            var order1 = new ExchangeOrder(1, 1, OrderSide.Buy, OrderType.Limit, 10, 5);
            var order2 = new ExchangeOrder(2, 1, OrderSide.Sell, OrderType.Limit, 11, 6);

            sourceOrderList.Add(order1);
            sourceOrderList.Add(order2);

            var resultOrderList = new List<ExchangeOrder>();
            var resultOrderBookList = new List<IOrderBook>();
            var orderStreamSubscription = orderStream.Subscribe(resultOrderList.Add);
            var orderBookStreamSubscription = orderBookStream.Subscribe(resultOrderBookList.Add);

            testScheduler.Start();
            mockExchangeClient.Connect();
            foreach (var order in sourceOrderList)
            {
                //do not wait for result.
                mockExchangeClient.SubmitOrder(order.OSide, order.OType, order.Price, order.Size);
            }
            mockExchangeClient.Disconnect();
            testScheduler.Stop();

            //test number of orders pushed
            Assert.AreEqual(sourceOrderList.Count, resultOrderList.Count);

            //test number of resulting orderbooks
            //there should be as many as orders
            Assert.AreEqual(sourceOrderList.Count, resultOrderBookList.Count);

        }

        /// <summary>
        /// Test Start and Stop random orders
        /// </summary>
        [TestMethod]
        public void TestStartAndStopRandomOrders()
        {

            var testScheduler = new TestScheduler();

            var mockExchangeClient = new MockClient(1, testScheduler);

            var orderStream = mockExchangeClient.OrderStream;
            var orderBookStream = mockExchangeClient.OrderBookStream;

            var resultOrders = new List<ExchangeOrder>();
            var resultOrderBooks = new List<IOrderBook>();

            var orderStreamSubscription = orderStream.Subscribe(resultOrders.Add);
            var orderBookStreamSubscription = orderBookStream.Subscribe(resultOrderBooks.Add);

            mockExchangeClient.Connect();
            mockExchangeClient.StartRandomOrders();

            testScheduler.AdvanceBy(1000000000);

            //count the number of items pushed while randomorders started
            int orderCountBeforeStart = resultOrders.Count;
            int orderBookCountBeforeStart = resultOrderBooks.Count;

            mockExchangeClient.StopRandomOrders();
            mockExchangeClient.Disconnect();

            //advance the time scheduler
            testScheduler.AdvanceBy(1000000000);

            //count the number of items pushed while randomorders stopped
            int orderCountAfterStop = resultOrders.Count;
            int orderBookCountAfterStop = resultOrderBooks.Count;

            //No items should have been pushed
            Assert.AreEqual(orderCountBeforeStart, orderCountAfterStop);
            Assert.AreEqual(orderBookCountBeforeStart, orderBookCountAfterStop);

        }
    }
}
