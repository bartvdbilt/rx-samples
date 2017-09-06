using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderBookAggregator.Objects;

namespace OrderBookAggregator.Tests
{
    [TestClass]
    public class MockOrderBookTests
    {
        [TestMethod]
        public void TestMockOrderBook()
        {
            var orderBook = new MockOrderBook(1);

            var b1 = new ExchangeOrder(1, 1, OrderSide.Buy, OrderType.Limit, 10, 10);
            orderBook = orderBook.PlaceOrder(b1);

            Assert.AreEqual(1, orderBook.Orders.Count);
            Assert.AreEqual(1, orderBook.Bids.Count);
            Assert.AreEqual(0, orderBook.Asks.Count);

            var b2 = new ExchangeOrder(2, 1, OrderSide.Buy, OrderType.Limit, 11, 10);
            orderBook = orderBook.PlaceOrder(b2);

            Assert.AreEqual(2, orderBook.Orders.Count);
            Assert.AreEqual(2, orderBook.Bids.Count);
            Assert.AreEqual(0, orderBook.Asks.Count);

            var a1 = new ExchangeOrder(3, 1, OrderSide.Sell, OrderType.Limit, 10.5m, 5);
            orderBook = orderBook.PlaceOrder(a1);

            //all orders, even completed, should be contained in the dictionary
            Assert.AreEqual(3, orderBook.Orders.Count);

            //best bid was only half filled
            Assert.AreEqual(2, orderBook.Bids.Count);

            //the sell order was fully filled. asks should be empty
            Assert.AreEqual(0, orderBook.Asks.Count);


            Assert.AreEqual(5, orderBook.Bids[0].RemainingVolume);

            //check the order dictionary is update
            Assert.AreEqual(5, orderBook.Orders[2].RemainingVolume);
            Assert.AreEqual(0, orderBook.Orders[3].RemainingVolume);

            var a2 = new ExchangeOrder(4, 1, OrderSide.Sell, OrderType.Limit, 10, 7);
            orderBook = orderBook.PlaceOrder(a2);

            Assert.AreEqual(4, orderBook.Orders.Count);

            //best bid was fully filled
            Assert.AreEqual(1, orderBook.Bids.Count);

            //the ask was  fully filled
            Assert.AreEqual(0, orderBook.Asks.Count);

            //the next best bid was partially filled
            Assert.AreEqual(8, orderBook.Bids[0].RemainingVolume);

            Assert.AreEqual(0, orderBook.Orders[2].RemainingVolume);
            Assert.AreEqual(0, orderBook.Orders[4].RemainingVolume);
            Assert.AreEqual(8, orderBook.Orders[1].RemainingVolume);

            var b3 = new ExchangeOrder(5, 1, OrderSide.Buy, OrderType.Limit, 9, 10);
            orderBook = orderBook.PlaceOrder(b3);
            var a3 = new ExchangeOrder(6, 1, OrderSide.Sell, OrderType.Limit, 10, 10);
            orderBook = orderBook.PlaceOrder(a3);

            Assert.AreEqual(6, orderBook.Orders.Count);
            //the best bid was totally filled
            Assert.AreEqual(1, orderBook.Bids.Count);
            Assert.AreEqual(0, orderBook.Orders[1].RemainingVolume);

            Assert.AreEqual(1, orderBook.Asks.Count);
            Assert.AreEqual(2, orderBook.Orders[6].RemainingVolume);

            //the second bid was not touched because too low
            Assert.AreEqual(10, orderBook.Bids[0].RemainingVolume);

        }
    }
}
