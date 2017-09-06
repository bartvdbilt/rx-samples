using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class AggregatedOrderBook 
    {

        public AggregatedOrderBook()
        {
            Bids = ImmutableSortedSet.Create(comparer: OrderComparer.DescBidComparer());
            Asks = ImmutableSortedSet.Create(comparer: OrderComparer.DescAskComparer());

        }

        public AggregatedOrderBook(ImmutableSortedSet<ExchangeOrder> bids, ImmutableSortedSet<ExchangeOrder> asks)
        {
            Bids = bids;
            Asks = asks;
        }

        public ImmutableSortedSet<ExchangeOrder> Bids { get; private set; }
        public ImmutableSortedSet<ExchangeOrder> Asks { get; private set; }

        /// <summary>
        /// Insert or replace orders from a given OrderBook.
        /// It is assumed that all the orders in the incoming OrderBook are from the same Exchange (identified by ExchangeID).
        /// Any order from the same Exchange will be removed before the incoming OrderBook is inserted
        /// </summary>
        /// <param name="orderBook">orderbook containing the orders which should be inserted in the aggregated orderbook.</param>
        /// <returns>AggregatedOrderBook where order from the same Exchange as orderBook as replaced with the ones in orderBook</returns>
        public AggregatedOrderBook InsertBook(IOrderBook orderBook)
        {
            int exchangeID = orderBook.ExchangeID;
            var modifiedAggregatedBids = Bids;
            var modifiedAggregatedAsks = Asks;
            
            var correspondingBids = Bids.Where(a => a.ExchangeID == exchangeID);
            modifiedAggregatedBids = Bids.Except(correspondingBids).Union(orderBook.Bids);

            var correspondingAsks = Asks.Where(a => a.ExchangeID == exchangeID);
            modifiedAggregatedAsks = Asks.Except(correspondingAsks).Union(orderBook.Asks);

            return new AggregatedOrderBook(modifiedAggregatedBids, modifiedAggregatedAsks);
        }

        public ArbitrageOpportunity LookForArbitrage()
        {
            ArbitrageOpportunity arbitrage = null;

            if (Bids.Count > 0 && Asks.Count > 0)
            {

                //1-- determine max volume of arbitrage
                int bi = 0;
                int ai = 0;

                var topBid = Bids[bi];
                var topAsk = Asks[ai];
                decimal arbVolume = 0;

                while (topBid.Price > topAsk.Price)
                {

                    if (topBid.RemainingVolume >= topAsk.RemainingVolume)
                    {
                        arbVolume += topAsk.RemainingVolume;
                        topBid = topBid.WithUpdatedRemainingVolume(topAsk.RemainingVolume);
                        if (++ai < Asks.Count)
                            topAsk = Asks[ai];
                        else
                            break;
                    }
                    else
                    {
                        arbVolume += topBid.RemainingVolume;
                        topAsk = topAsk.WithUpdatedRemainingVolume(topBid.RemainingVolume);
                        if (++bi < Bids.Count)
                            topBid = Bids[bi];
                        else
                            break;
                    }
                }

                //2-- determine size of orders on each exchange.
                if (arbVolume > 0)
                {
                    Dictionary<int, decimal> buyOrdersByExchangeID = new Dictionary<int, decimal>();  //[exchangeID, amount] amount to buy at market price at exchange which has Id exhangeID                  
                    decimal tempBuyVolume = 0;
                    int askIndex = 0;
                    while (tempBuyVolume < arbVolume
                        && askIndex < Asks.Count)
                    {
                        var increment = Math.Min(Asks[askIndex].RemainingVolume, arbVolume - tempBuyVolume);
                        buyOrdersByExchangeID.AddOrUpdate(Asks[askIndex].ExchangeID, increment);
                        tempBuyVolume += increment;
                        askIndex++;
                    }

                    Dictionary<int, decimal> sellOrdersByExchangeID = new Dictionary<int, decimal>();  //[exchangeID, amount] amount to sell at market price at exchange which has Id exhangeID
                    decimal tempSellVolume = 0;
                    int bidIndex = 0;
                    while (tempSellVolume < arbVolume
                        && bidIndex < Bids.Count)
                    {
                        var increment = Math.Min(Bids[bidIndex].RemainingVolume, arbVolume - tempSellVolume);
                        sellOrdersByExchangeID.AddOrUpdate(Bids[bidIndex].ExchangeID, increment);
                        tempSellVolume += increment;
                        bidIndex++;
                    }

                    arbitrage = new ArbitrageOpportunity(buyOrdersByExchangeID, sellOrdersByExchangeID);
                }
            }

            return arbitrage;
        }

        public override string ToString()
        {
            string res = "\n";
            res += string.Format("\n{0,-27}    {1, -27}\n","===BIDS===","===ASKS===");
            res += string.Format("\n{0,-8} {1,-7} {2, -10} || {0,-8} {1,-7} {2,-10}\n","Exchange","Price","Amount","");
 
            using (var bidEnumerator = Bids.GetEnumerator())
            using (var askEnumerator = Asks.GetEnumerator())
            {

                var nextBid = bidEnumerator.MoveNext();
                var nextAsk = askEnumerator.MoveNext();

                while (nextBid || nextAsk)
                {
                    string nextBidExchange = (nextBid) ? bidEnumerator.Current.ExchangeID.ToString() : "";
                    string nextBidPrice = (nextBid) ? bidEnumerator.Current.Price.ToString() : "";
                    string nextBidAmount = (nextBid) ? bidEnumerator.Current.RemainingVolume.ToString("N4") : "";

                    string nextAskExchange = (nextAsk) ? askEnumerator.Current.ExchangeID.ToString() : "";
                    string nextAskPrice = (nextAsk) ? askEnumerator.Current.Price.ToString() : "";
                    string nextAskAmount = (nextAsk) ? askEnumerator.Current.RemainingVolume.ToString("N4") : "";
                    
                    res += string.Format("\n{0,-8} {1,-7} {2,-10} || {3,-8} {4,-7} {5,-10}\n", nextBidExchange, nextBidPrice, nextBidAmount, nextAskExchange, nextAskPrice, nextAskAmount);

                    nextBid = bidEnumerator.MoveNext();
                    nextAsk = askEnumerator.MoveNext();
                }
            }            
            return res;
        }
    }

    public static class DictionaryExtensions
    {
        public static void AddOrUpdate(this Dictionary<int, decimal> dictionary, int key, decimal increment)
        {
            decimal value;
            if (dictionary.TryGetValue(key, out value))
            {
                dictionary[key] = value + increment;
            }
            else
            {
                dictionary[key] = increment;
            }
        }
    }
}
