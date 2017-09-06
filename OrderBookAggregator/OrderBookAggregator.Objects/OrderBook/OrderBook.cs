using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class OrderBook : IOrderBook
    {
        public OrderBook(int exchnangeID, IEnumerable<ExchangeOrder> bids, IEnumerable<ExchangeOrder> asks)
        {
            ExchangeID = exchnangeID;
            Bids = bids.ToImmutableSortedSet(comparer: OrderComparer.DescBidComparer());
            Asks = asks.ToImmutableSortedSet(comparer: OrderComparer.DescAskComparer());
        }

        public int ExchangeID { get; private set; }
        public ImmutableSortedSet<ExchangeOrder> Bids { get; private set; }
        public ImmutableSortedSet<ExchangeOrder> Asks { get; private set; }
    }
}
