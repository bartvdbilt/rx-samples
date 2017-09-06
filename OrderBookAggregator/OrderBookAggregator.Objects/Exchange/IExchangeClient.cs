using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public interface IExchangeClient
    {
        int ExchangeID
        {
            get;
        }

        IObservable<ExchangeOrder> OrderStream
        {
            get;
        }

        IObservable<IOrderBook> OrderBookStream
        {
            get;
        }

        Task<ExchangeOrder> SubmitOrder(OrderSide oSide, OrderType oType, decimal price, decimal size);

        Task<ExchangeOrder> CancelOrder(ExchangeOrder order);

        void Connect();

        void Disconnect();
    }
}
