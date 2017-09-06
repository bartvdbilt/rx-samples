using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class ExchangeOrder:IComparable<ExchangeOrder>
    {
        public int ID { get; private set; }
        public int ExchangeID { get; private set; }
        public OrderSide OSide { get; private set; } //buy,sell
        public OrderType OType { get; private set; }//limit,market
        public decimal Price { get; private set; }
        public decimal Size { get; private set; }
        public decimal RemainingVolume { get; private set; }
        public ExchangeOrderStatus Status { get; private set; }
        public DateTimeOffset UTCTimestamp { get; private set; }

        public ExchangeOrder() { }

        public ExchangeOrder(int id, int exchangeID, OrderSide oSide, OrderType oType, decimal price, decimal size) : this(id, exchangeID, oSide, oType, price, size, size, ExchangeOrderStatus.Pending, DateTimeOffset.UtcNow) { }

        public ExchangeOrder(int id, int exchangeID, OrderSide oSide, OrderType oType, decimal price, decimal size, decimal remainingVolume, ExchangeOrderStatus status, DateTimeOffset utcTimestamp)
        {
            ID = id;
            ExchangeID = exchangeID;
            OSide = oSide;
            OType = oType;
            Price = price;
            Size = size;
            RemainingVolume = remainingVolume;
            Status = status;
            UTCTimestamp = utcTimestamp;
        }

        public ExchangeOrder WithUpdatedRemainingVolume(decimal lastTradeAmount)
        {
            var remainingVolume = this.RemainingVolume - lastTradeAmount;

            ExchangeOrderStatus newStatus = this.Status;
            if (remainingVolume == 0)
                newStatus = ExchangeOrderStatus.Closed;

            //this is important. When we want to update an item in bids or asks, we remove it, change its remaining volume by calling this method, and add it back into the sorted set.
            //If the timestamp were to change, the order might loose it's spot in the queue.
            var utcTimestamp = this.UTCTimestamp;

            var order = new ExchangeOrder(
                this.ID,
                this.ExchangeID,
                this.OSide,
                this.OType,
                this.Price,
                this.Size,
                remainingVolume,
                newStatus,
                utcTimestamp);
            return order;
        }

        public ExchangeOrder WithUpdatedStatus(ExchangeOrderStatus newStatus)
        {
            var order = new ExchangeOrder(
                    this.ID,
                    this.ExchangeID,
                    this.OSide,
                    this.OType,
                    this.Price,
                    this.Size,
                    this.RemainingVolume,
                    newStatus,
                    this.UTCTimestamp);
            return order;
        }

        //General comparer. Doesnt care about ordering bid ask
        public int CompareTo(ExchangeOrder x)
        {

            if (this.ID.CompareTo(x.ID) != 0)
            {
                return this.ID.CompareTo(x.ID);
            }
            if (this.ExchangeID.CompareTo(x.ExchangeID) != 0)
            {
                return this.ExchangeID.CompareTo(x.ExchangeID);
            }
            if (this.OType.CompareTo(x.OType) != 0)
            {
                return this.OType.CompareTo(x.OType);
            }
            if (this.OSide.CompareTo(x.OSide) != 0)
            {
                return this.OSide.CompareTo(x.OSide);
            }
            if (this.Price.CompareTo(x.Price) != 0)
            {
                return this.Price.CompareTo(x.Price);
            }
            if (this.UTCTimestamp.CompareTo(x.UTCTimestamp) != 0)
            {
                return this.UTCTimestamp.CompareTo(x.UTCTimestamp);
            }
            if (this.RemainingVolume.CompareTo(x.RemainingVolume) != 0)
            {
                return this.RemainingVolume.CompareTo(x.RemainingVolume);
            }
            if (this.Status.CompareTo(x.Status) != 0)
            {
                return this.Status.CompareTo(x.Status);
            }
            //its the same order
            return 0;
        }

        public override string ToString()
        {
            return string.Format("ExchangeID: {0} | ID: {1} | Side: {2} |  Price: {3} | Size: {4} | RV: {5} | Status: {6} | TS: {7} \n", ExchangeID, ID, OSide, Price, Size, RemainingVolume, Status, UTCTimestamp);
        }
    }

    public enum OrderSide
    {
        Buy = 1,
        Sell = 2
    }

    public enum OrderType
    {
        Limit = 1,
        Market = 2
    }

    public enum ExchangeOrderStatus
    {
        Pending = 1, //order pending book entry
        Open = 2,
        Closed = 3,
        Canceled = 4
    }
}
