using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class OrderComparer : Comparer<ExchangeOrder>
    {
        //-1 for Bid comparer
        //1 for Ask comparer
        readonly int _priceComparisonCoeff;

        OrderComparer(int priceComparisonCoeff)
        {
            _priceComparisonCoeff = priceComparisonCoeff;
        }

        //!! counterintuitive but we need SortedSets to be in descending order. Instead of calling reverse all the time, we implement the behavior in the comparer
        //Returns: -1 if x is better than y
        //          0 if x is equivalent to y
        //          1 if x is worse than y
        //
        //         bids and asks:> market order always wins over limit order
        //            if both market orders and same timestamp, most remaining volume wins         
        //        
        //         bids:> most expensive wins  
        //         asks:> cheapest wins,
        //            if same price, oldest wins,
        //                if same price and timestamp, most remaining volume wins
        //
        //ASSUMPTION: x and y are on the same side (BID or ASK).
        //We do not look at ID or ExchangeID
        //The order of comparisons is: price, timestamp, remainingvolume (note that size does not matter)
        public override int Compare(ExchangeOrder x, ExchangeOrder y)
        {

            //one market order and one limit order
            if (x.OType == OrderType.Market && y.OType == OrderType.Limit)
            {
                return -1;
            }
            if (x.OType == OrderType.Limit && y.OType == OrderType.Market)
            {
                return 1;
            }

            //two limit orders
            if (x.Price.CompareTo(y.Price) != 0)
            {
                return _priceComparisonCoeff * x.Price.CompareTo(y.Price);
            }
            if (x.UTCTimestamp.CompareTo(y.UTCTimestamp) != 0)
            {
                return x.UTCTimestamp.CompareTo(y.UTCTimestamp);
            }

            //two market orders or two limit orders with the same prices
            if (x.RemainingVolume.CompareTo(y.RemainingVolume) != 0)
            {
                return -x.RemainingVolume.CompareTo(y.RemainingVolume);
            }

            //they have the same characteristics. not necessary same ID
            //not good because we are not supposed to have two equivalent orders in the orderbook
            return 0;
        }

        public static OrderComparer DescBidComparer()
        {
            return new OrderComparer(-1);
        }

        public static OrderComparer DescAskComparer()
        {
            return new OrderComparer(1);
        }

    }
}
