using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderBookAggregator.Objects;
using System.Threading;
using System.Collections.Immutable;

namespace OrderBookAggregator.Tests
{
    [TestClass]
    public class OrderTests
    {
        [TestMethod]
        public void TestBidComparer()
        {

            //Remember this is descending order
            var bidComparer = OrderComparer.DescBidComparer();

            var o1 = new ExchangeOrder(1, 1, OrderSide.Buy, OrderType.Limit, 10, 10);
            var o2 = new ExchangeOrder(2, 1, OrderSide.Buy, OrderType.Limit, 9, 10);

            var c1 = bidComparer.Compare(o1, o2);

            Assert.AreEqual(-1, c1, string.Format("BidComparer: higher price should win over lower price. {0} vs {1}", o1.Price, o2.Price));

            var c2 = bidComparer.Compare(o2, o1);

            Assert.AreEqual(1, c2, "BidComparer: price comparison not symmetric");

            Thread.Sleep(TimeSpan.FromTicks(10000)); //otherwhise timestamp comparison is not precise enough
            var o3 = new ExchangeOrder(3, 1, OrderSide.Buy, OrderType.Limit, 10, 10);

            var c3 = bidComparer.Compare(o1, o3);

            //this test does not always pass. if timespan between c1 and c2 is too small.
            Assert.AreEqual(-1, c3, string.Format("BidComparer: Timestamp comparison failed. {0} vs {1}", o1.UTCTimestamp.Ticks, o3.UTCTimestamp.Ticks));

            var c4 = bidComparer.Compare(o3, o1);

            Assert.AreEqual(1, c4, "BidComparer: Timestamp comparison not symmetric");

            //try to get two orders with the same timestamp.
            //depending on environment, it could take a variable amout of time. Try 10 times and then give up
            var o4 = new ExchangeOrder(4, 1, OrderSide.Buy, OrderType.Limit, 10, 10);
            var o5 = new ExchangeOrder(5, 1, OrderSide.Buy, OrderType.Limit, 10, 9);
            int counter = 0;
            while (o4.UTCTimestamp != o5.UTCTimestamp && counter < 10)
            {
                o4 = new ExchangeOrder(4, 1, OrderSide.Buy, OrderType.Limit, 10, 10);
                o5 = new ExchangeOrder(5, 1, OrderSide.Buy, OrderType.Limit, 10, 9);
                counter++;
            }

            if (counter == 10)
                Assert.Inconclusive("failed to create two orders with the same timestamp. Could not test RemainingVolume comparison");
            else
            {
                var c5 = bidComparer.Compare(o4, o5);
                Assert.AreEqual(-1, c5, string.Format("BidComparer: If price and timestamp are equal, the order with the most remaining volume should win. {0} vs {1}", o4.RemainingVolume, o5.RemainingVolume));
            }
        }

        [TestMethod]
        public void TestAskComparer()
        {
            var askComparer = OrderComparer.DescAskComparer();

            var o1 = new ExchangeOrder(1, 1, OrderSide.Sell, OrderType.Limit, 10, 10);
            var o2 = new ExchangeOrder(2, 1, OrderSide.Sell, OrderType.Limit, 9, 10);

            var c1 = askComparer.Compare(o1, o2);

            Assert.AreEqual(1, c1, string.Format("AskComparer: lower price should win over higher price. {0} vs {1}", o1.Price, o2.Price));

            var c2 = askComparer.Compare(o2, o1);

            Assert.AreEqual(-1, c2, "AskComparer: price comparison not symmetric");


            Thread.Sleep(TimeSpan.FromTicks(10000)); //otherwhise timestamp comparison is not precise enough
            var o3 = new ExchangeOrder(3, 1, OrderSide.Sell, OrderType.Limit, 10, 10);

            var c3 = askComparer.Compare(o1, o3);

            //this test does not always pass. if timespan between c1 and c2 is too small.
            Assert.AreEqual(-1, c3, string.Format("AskComparer: Timestamp comparison failed. {0} vs {1}", o1.UTCTimestamp.Ticks, o3.UTCTimestamp.Ticks));

            var c4 = askComparer.Compare(o3, o1);

            Assert.AreEqual(1, c4, "BidComparer: Timestamp comparison not symmetric");

            //try to get two orders with the same timestamp.
            //depending on environment, it could take a variable amout of time. Try 10 times and then give up
            var o4 = new ExchangeOrder(4, 1, OrderSide.Sell, OrderType.Limit, 10, 10);
            var o5 = new ExchangeOrder(5, 1, OrderSide.Sell, OrderType.Limit, 10, 9);
            int counter = 0;
            while (o4.UTCTimestamp != o5.UTCTimestamp && counter < 10)
            {
                o4 = new ExchangeOrder(4, 1, OrderSide.Sell, OrderType.Limit, 10, 10);
                o5 = new ExchangeOrder(5, 1, OrderSide.Sell, OrderType.Limit, 10, 9);
                counter++;
            }

            if (counter == 10)
                Assert.Inconclusive("failed to create two orders with the same timestamp. Could not test RemainingVolume comparison");
            else
            {
                var c5 = askComparer.Compare(o4, o5);
                Assert.AreEqual(-1, c5, string.Format("AskComparer: If price and timestamp are equal, the order with the most remaining volume should win. {0} vs {1}", o4.RemainingVolume, o5.RemainingVolume));
            }
        }

        [TestMethod]
        public void TestSortedSetOfOrders()
        {

            //ATTENTION sorted set is in descending order. [best order, ..., worst order]
            var bids = ImmutableSortedSet.Create(comparer: OrderComparer.DescBidComparer());

            var b1 = new ExchangeOrder(1, 1, OrderSide.Buy, OrderType.Limit, 10, 10);
            bids = bids.Add(b1);
            var b2 = new ExchangeOrder(2, 1, OrderSide.Buy, OrderType.Limit, 9, 3);
            bids = bids.Add(b2);
            Assert.AreEqual(1, bids[0].ID);
            Assert.AreEqual(2, bids[1].ID);

            var b3 = new ExchangeOrder(3, 1, OrderSide.Buy, OrderType.Limit, 11, 5);
            bids = bids.Add(b3);
            Assert.AreEqual(3, bids[0].ID);
            Assert.AreEqual(1, bids[1].ID);
            Assert.AreEqual(2, bids[2].ID);

            var b4 = new ExchangeOrder(4, 1, OrderSide.Buy, OrderType.Limit, 10, 8);
            bids = bids.Add(b4);
            Assert.AreEqual(3, bids[0].ID);
            Assert.AreEqual(1, bids[1].ID);
            Assert.AreEqual(4, bids[2].ID);
            Assert.AreEqual(2, bids[3].ID);

            var b5 = new ExchangeOrder(5, 1, OrderSide.Buy, OrderType.Limit, 9.5m, 4);
            bids = bids.Add(b5);
            Assert.AreEqual(3, bids[0].ID);
            Assert.AreEqual(1, bids[1].ID);
            Assert.AreEqual(4, bids[2].ID);
            Assert.AreEqual(5, bids[3].ID);
            Assert.AreEqual(2, bids[4].ID);


            var asks = ImmutableSortedSet.Create(comparer: OrderComparer.DescAskComparer());

            var a1 = new ExchangeOrder(1, 1, OrderSide.Sell, OrderType.Limit, 10, 10);
            asks = asks.Add(a1);
            var a2 = new ExchangeOrder(2, 1, OrderSide.Sell, OrderType.Limit, 11, 4);
            asks = asks.Add(a2);
            Assert.AreEqual(1, asks[0].ID);
            Assert.AreEqual(2, asks[1].ID);

            var a3 = new ExchangeOrder(3, 1, OrderSide.Sell, OrderType.Limit, 9, 5);
            asks = asks.Add(a3);
            Assert.AreEqual(3, asks[0].ID);
            Assert.AreEqual(1, asks[1].ID);
            Assert.AreEqual(2, asks[2].ID);

            var a4 = new ExchangeOrder(4, 1, OrderSide.Sell, OrderType.Limit, 10, 8);
            asks = asks.Add(a4);
            Assert.AreEqual(3, asks[0].ID);
            Assert.AreEqual(1, asks[1].ID);
            Assert.AreEqual(4, asks[2].ID);
            Assert.AreEqual(2, asks[3].ID);

            var a5 = new ExchangeOrder(5, 1, OrderSide.Sell, OrderType.Limit, 10.5m, 2);
            asks = asks.Add(a5);
            Assert.AreEqual(3, asks[0].ID);
            Assert.AreEqual(1, asks[1].ID);
            Assert.AreEqual(4, asks[2].ID);
            Assert.AreEqual(5, asks[3].ID);
            Assert.AreEqual(2, asks[4].ID);

        }
    }
}
