using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class ArbitrageOpportunity
    {
        public ArbitrageOpportunity(Dictionary<int, decimal> buyDico, Dictionary<int, decimal> sellDico)
        {
            BuyDico = buyDico;
            SellDico = sellDico;
        }

        /// <summary>
        /// dictionary of [exchangeID, buyVolume] representing the amounts that should be bought on each exchange
        /// </summary>
        public Dictionary<int, decimal> BuyDico { get; private set; }

        /// <summary>
        /// dictionary of [exchangeID, sellVolume] representing the amounts that should be sold on each exchange
        /// </summary>
        public Dictionary<int, decimal> SellDico { get; private set; }

    }
}
