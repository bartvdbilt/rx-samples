using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderBookAggregator.Objects;

namespace OrderBookAggregator.Tests
{

    [TestClass]
    public class AggregatedOrderBookTests
    {


        [TestMethod]
        public void TestAggregatedOrderBook()
        {
            #region Check Union of sorted sets is still sorted

            var o11 = new ExchangeOrder(1, 1, OrderSide.Buy, OrderType.Limit, 10, 5);
            var o12 = new ExchangeOrder(2, 1, OrderSide.Buy, OrderType.Limit, 9, 4);
            var o13 = new ExchangeOrder(3, 1, OrderSide.Sell, OrderType.Limit, 12, 6);
            var o14 = new ExchangeOrder(4, 1, OrderSide.Sell, OrderType.Limit, 11, 7);
            var orderBook1 = new MockOrderBook(1).PlaceOrder(o11)
                                            .PlaceOrder(o12)
                                            .PlaceOrder(o13)
                                            .PlaceOrder(o14);

            var o21 = new ExchangeOrder(5, 2, OrderSide.Buy, OrderType.Limit, 10.5m, 5.5m);
            var o22 = new ExchangeOrder(6, 2, OrderSide.Buy, OrderType.Limit, 9.5m, 4.5m);
            var o23 = new ExchangeOrder(7, 2, OrderSide.Sell, OrderType.Limit, 12.5m, 5.5m);
            var o24 = new ExchangeOrder(8, 2, OrderSide.Sell, OrderType.Limit, 11.5m, 6.5m);
            var orderBook2 = new MockOrderBook(2).PlaceOrder(o21)
                                            .PlaceOrder(o22)
                                            .PlaceOrder(o23)
                                            .PlaceOrder(o24);

            var aggregatedOrderBook = new AggregatedOrderBook().InsertBook(orderBook1)
                                                                .InsertBook(orderBook2);

            Assert.AreEqual(5, aggregatedOrderBook.Bids[0].ID);
            Assert.AreEqual(1, aggregatedOrderBook.Bids[1].ID);
            Assert.AreEqual(6, aggregatedOrderBook.Bids[2].ID);
            Assert.AreEqual(2, aggregatedOrderBook.Bids[3].ID);

            Assert.AreEqual(4, aggregatedOrderBook.Asks[0].ID);
            Assert.AreEqual(8, aggregatedOrderBook.Asks[1].ID);
            Assert.AreEqual(3, aggregatedOrderBook.Asks[2].ID);
            Assert.AreEqual(7, aggregatedOrderBook.Asks[3].ID);

            #endregion

            #region Check replacing an order book

            var o222 = o22.WithUpdatedRemainingVolume(1);
            var o232 = o23.WithUpdatedRemainingVolume(0.5m);
            var o242 = o24.WithUpdatedRemainingVolume(0.2m);
            var o25 = new ExchangeOrder(9, 2, OrderSide.Sell, OrderType.Limit, 13, 10);
            var orderBook22 = new MockOrderBook(2).PlaceOrder(o222)
                                             .PlaceOrder(o232)
                                             .PlaceOrder(o242)
                                             .PlaceOrder(o25);

            var aggregatedOrderBook2 = aggregatedOrderBook.InsertBook(orderBook22);

            Assert.AreEqual(1, aggregatedOrderBook2.Bids[0].ID);
            Assert.AreEqual(6, aggregatedOrderBook2.Bids[1].ID);
            Assert.AreEqual(3.5m, aggregatedOrderBook2.Bids[1].RemainingVolume);
            Assert.AreEqual(2, aggregatedOrderBook2.Bids[2].ID);


            Assert.AreEqual(4, aggregatedOrderBook2.Asks[0].ID);
            Assert.AreEqual(8, aggregatedOrderBook2.Asks[1].ID);
            Assert.AreEqual(6.3m, aggregatedOrderBook2.Asks[1].RemainingVolume);
            Assert.AreEqual(3, aggregatedOrderBook2.Asks[2].ID);
            Assert.AreEqual(7, aggregatedOrderBook2.Asks[3].ID);
            Assert.AreEqual(5, aggregatedOrderBook2.Asks[3].RemainingVolume);
            Assert.AreEqual(9, aggregatedOrderBook2.Asks[4].ID);

            #endregion

            #region Check arbitrage scanning

            var o31 = new ExchangeOrder(1, 3, OrderSide.Buy, OrderType.Limit, 12, 5);
            var o32 = new ExchangeOrder(2, 3, OrderSide.Buy, OrderType.Limit, 9, 7);
            var o33 = new ExchangeOrder(3, 3, OrderSide.Sell, OrderType.Limit, 13, 4);
            var o34 = new ExchangeOrder(4, 3, OrderSide.Sell, OrderType.Limit, 14, 6);
            var orderBook3 = new MockOrderBook(3).PlaceOrder(o31)
                                            .PlaceOrder(o32)
                                            .PlaceOrder(o33)
                                            .PlaceOrder(o34);

            var o41 = new ExchangeOrder(5, 4, OrderSide.Buy, OrderType.Limit, 10.5m, 2);
            var o42 = new ExchangeOrder(6, 4, OrderSide.Buy, OrderType.Limit, 8, 6);
            var o43 = new ExchangeOrder(7, 4, OrderSide.Sell, OrderType.Limit, 11, 8);
            var o44 = new ExchangeOrder(8, 4, OrderSide.Sell, OrderType.Limit, 13.5m, 2);
            var orderBook4 = new MockOrderBook(4).PlaceOrder(o41)
                                            .PlaceOrder(o42)
                                            .PlaceOrder(o43)
                                            .PlaceOrder(o44);

            var aggregatedOrderBook3 = new AggregatedOrderBook().InsertBook(orderBook3)
                                                                .InsertBook(orderBook4);

            var arbitrage = aggregatedOrderBook3.LookForArbitrage();

            Assert.AreEqual(5, arbitrage.BuyDico[4]);
            Assert.AreEqual(5, arbitrage.SellDico[3]);


            #endregion

            #region Check arbitrage scanning 2

            var o51 = new ExchangeOrder(1, 5, OrderSide.Buy, OrderType.Limit, 13, 5);
            var o52 = new ExchangeOrder(2, 5, OrderSide.Buy, OrderType.Limit, 12.8m, 2);
            var o53 = new ExchangeOrder(3, 5, OrderSide.Buy, OrderType.Limit, 12.7m, 6);
            var o54 = new ExchangeOrder(4, 5, OrderSide.Buy, OrderType.Limit, 12, 4);
            var orderBook5 = new MockOrderBook(5).PlaceOrder(o51)
                .PlaceOrder(o52)
                .PlaceOrder(o53)
                .PlaceOrder(o54);

            var o61 = new ExchangeOrder(1, 6, OrderSide.Sell, OrderType.Limit, 12.5m, 10);
            var o62 = new ExchangeOrder(2, 6, OrderSide.Sell, OrderType.Limit, 13, 4);
            var orderBook6 = new MockOrderBook(6).PlaceOrder(o61)
                .PlaceOrder(o62);

            var o71 = new ExchangeOrder(1, 7, OrderSide.Sell, OrderType.Limit, 12, 7);
            var o72 = new ExchangeOrder(2, 7, OrderSide.Sell, OrderType.Limit, 12.7m, 5);
            var orderBook7 = new MockOrderBook(7).PlaceOrder(o71)
                .PlaceOrder(o72);

            var aggregatedOrderBook4 = new AggregatedOrderBook().InsertBook(orderBook5)
                .InsertBook(orderBook6)
                .InsertBook(orderBook7);

            var arbitrage2 = aggregatedOrderBook4.LookForArbitrage();

            Assert.AreEqual(7, arbitrage2.BuyDico[7]);
            Assert.AreEqual(6, arbitrage2.BuyDico[6]);
            Assert.AreEqual(13, arbitrage2.SellDico[5]);

            #endregion
        }
    }
    
}
