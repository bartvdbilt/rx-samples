using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public interface IOrderBook
    {
        int ExchangeID 
        { 
            get;
        }

        ImmutableSortedSet<ExchangeOrder> Bids
        {
            get;
        }

        ImmutableSortedSet<ExchangeOrder> Asks
        {
            get;
        }
    }
}
