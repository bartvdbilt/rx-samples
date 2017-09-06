using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.Objects
{
    public class MockOrderBook : IOrderBook
    {
        public MockOrderBook(int exchangeID)
        {
            ExchangeID = exchangeID;
            Orders = ImmutableDictionary<int, ExchangeOrder>.Empty;
            Bids = ImmutableSortedSet.Create(comparer: OrderComparer.DescBidComparer());
            Asks = ImmutableSortedSet.Create(comparer: OrderComparer.DescAskComparer());
        }

        public MockOrderBook(int exchangeID,
                             ImmutableDictionary<int, ExchangeOrder> orders,
                             ImmutableSortedSet<ExchangeOrder> orderedBids,
                             ImmutableSortedSet<ExchangeOrder> orderedAsks)
        {
            ExchangeID = exchangeID;
            Orders = orders;
            Bids = orderedBids;
            Asks = orderedAsks;
        }

        public int ExchangeID {get;private set;}
        //The Bids and Asks collections contain both Limit and Market orders. The market orders will alway be on top. 
        //It is up to the consumer (whoever is looking at the orderbook) to decide wether to display the Market orders or not.
        public readonly ImmutableDictionary<int, ExchangeOrder> Orders;
        public ImmutableSortedSet<ExchangeOrder> Bids { get; private set; }
        public ImmutableSortedSet<ExchangeOrder> Asks { get; private set; }

        public MockOrderBook HandleOrder(ExchangeOrder incomingOrder)
        {
            switch (incomingOrder.Status)
            {
                case ExchangeOrderStatus.Canceled:
                    return CancelOrder(incomingOrder);
                case ExchangeOrderStatus.Pending:
                    return PlaceOrder(incomingOrder);
                default:
                    throw new Exception(string.Format("Unhandled incoming order status {0}", incomingOrder.Status));
            }
        }

        public MockOrderBook PlaceOrder(ExchangeOrder pendingOrder)
        {

            if (pendingOrder == null)
                throw new ArgumentNullException("pendingOrder");
            if (pendingOrder.Status != ExchangeOrderStatus.Pending)
                throw new Exception(string.Format("Cannot insert order with '{0}' status in the OrderBook", pendingOrder.Status));

            //Now the order is opened
            var openOrder = pendingOrder.WithUpdatedStatus(ExchangeOrderStatus.Open);

            var orders = this.Orders;
            var bids = this.Bids;
            var asks = this.Asks;

            ImmutableSortedSet<ExchangeOrder> oppositeSideOrders = null;
            ImmutableSortedSet<ExchangeOrder> sameSideOrders = null;

            if (openOrder.OSide == OrderSide.Buy)
            {
                oppositeSideOrders = asks;
                sameSideOrders = bids;
            }
            else
            {
                oppositeSideOrders = bids;
                sameSideOrders = asks;
            }

            var matchOuput = MatchOrder(Tuple.Create(openOrder, oppositeSideOrders));

            ExchangeOrder resultingOrder = matchOuput.Item1;
            ImmutableSortedSet<ExchangeOrder> resultingSameSideOrders = null;
            if (resultingOrder.RemainingVolume > 0)
            {
                resultingSameSideOrders = sameSideOrders.Add(resultingOrder);
            }
            else
            {
                resultingSameSideOrders = sameSideOrders;
            }

            ImmutableSortedSet<ExchangeOrder> resultingOppositeSideOrders = matchOuput.Item2;

            #region update orders
            var ordersBuilder = orders.ToBuilder();

            ordersBuilder[resultingOrder.ID] = resultingOrder;

            var oppositeSideOrdersDiff = resultingOppositeSideOrders.SymmetricExcept(oppositeSideOrders).//contains all the elements which are NOT PRESENT IN BOTH sets. for updated orders, both the old and new one will be returned
                                            GroupBy(o => o.ID, (_, g) => g.OrderBy(o => o.RemainingVolume).FirstOrDefault()).//for changed orders, take only the latest one
                                            ToList();

            foreach (var item in oppositeSideOrdersDiff)
            {
                //add or update the value in dictionary
                ordersBuilder[item.ID] = item;
            }
            #endregion

            #region reconstruct bids and asks
            ImmutableSortedSet<ExchangeOrder> resultingBids = null;
            ImmutableSortedSet<ExchangeOrder> resultingAsks = null;

            if (openOrder.OSide == OrderSide.Buy)
            {
                resultingBids = resultingSameSideOrders;
                resultingAsks = resultingOppositeSideOrders;
            }
            else
            {
                resultingBids = resultingOppositeSideOrders;
                resultingAsks = resultingSameSideOrders;
            }

            resultingBids = resultingBids.Where(o => o.Status == ExchangeOrderStatus.Open).ToImmutableSortedSet(OrderComparer.DescBidComparer());
            resultingAsks = resultingAsks.Where(o => o.Status == ExchangeOrderStatus.Open).ToImmutableSortedSet(OrderComparer.DescAskComparer());

            #endregion

            //return all orders + bids and asks from which completed orders are removed
            return new MockOrderBook(this.ExchangeID,
                                    ordersBuilder.ToImmutable(),
                                    resultingBids,
                                    resultingAsks);

        }

        Tuple<ExchangeOrder, ImmutableSortedSet<ExchangeOrder>> MatchOrder(Tuple<ExchangeOrder, ImmutableSortedSet<ExchangeOrder>> input)
        {

            if (input.Item2 == null || input.Item2.Count == 0)
                return input;

            var incomingOrder = input.Item1;
            var targetOrders = input.Item2.ToArray();

            int nextBestTargetIndex = 0;

            while (incomingOrder.RemainingVolume > 0
                && nextBestTargetIndex < targetOrders.Count())
            {

                var bestTarget = targetOrders.ElementAt(nextBestTargetIndex);

                decimal transactionPrice = GetTransactionPrice(incomingOrder, bestTarget);//returns -1 if the orders dont match (if there is no transaction)

                if (transactionPrice >= 0)
                {
                    //there was a match

                    //determine the volume of the transaction (fill as much as possible)
                    var transactionVolume = Math.Min(incomingOrder.RemainingVolume, bestTarget.RemainingVolume);

                    //Update the remaining volume and status on orders
                    incomingOrder = incomingOrder.WithUpdatedRemainingVolume(transactionVolume);

                    var updatedBestTarget = bestTarget.WithUpdatedRemainingVolume(transactionVolume);

                    targetOrders[nextBestTargetIndex] = updatedBestTarget;

                    //move on to the next target
                    nextBestTargetIndex++;
                }
                else
                {
                    //if there is no transaction, it is not necessary to go deeper in the target orders because they are already sorted
                    //and if there was no transaction now, there wont be any later
                    break;
                }

            }

            return Tuple.Create(incomingOrder, targetOrders.ToImmutableSortedSet());//it doesnt matter what comparer we pass to targetOrders because we will not need to sort it after this call
        }

        //if the bid > ask or one and only of the orders is a market order, return the price of the transaction
        //else return -1
        //two birds one stone:
        //allows me to know IF there was a transaction and if so, tells me what the price is
        private decimal GetTransactionPrice(ExchangeOrder incomingOrder, ExchangeOrder targetOrder)
        {
            if (incomingOrder.OSide == targetOrder.OSide)
                throw new ArgumentException("Cannot compute transaction price for two same-side orders");

            if (incomingOrder.Status != ExchangeOrderStatus.Open || targetOrder.Status != ExchangeOrderStatus.Open)
                throw new Exception("A transaction price can only be evaluated between two Open orders");

            decimal price = -1;

            if (!((incomingOrder.OType == OrderType.Market) && (targetOrder.OType == OrderType.Market)))//there is no transaction if they are two market orders
            {

                if (incomingOrder.OType == OrderType.Market)
                {
                    //at this point we are sure the target order is a limit order
                    price = targetOrder.Price;
                }
                else if (targetOrder.OType == OrderType.Market)
                {
                    //at this point we are sure the incoming order is a limit order
                    price = incomingOrder.Price;
                }
                else
                {
                    //they are both limit orders
                    //we choose the price that favors the target order because it arrived first (first come best served)

                    if (((incomingOrder.OSide == OrderSide.Buy) && (incomingOrder.Price >= targetOrder.Price))
                     || ((incomingOrder.OSide == OrderSide.Sell) && (incomingOrder.Price <= targetOrder.Price))
                    )
                    {
                        price = incomingOrder.Price;
                    }
                }
            }

            return price;
        }

        public MockOrderBook CancelOrder(ExchangeOrder order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var orders = this.Orders;
            var bids = this.Bids;
            var asks = this.Asks;

            #region remove order from bids or asks
            if (order.OSide == OrderSide.Buy)
            {
                var bidsBuilder = bids.ToBuilder();
                var orderToRemove = bidsBuilder.SingleOrDefault(o => o.ID == order.ID);
                if (orderToRemove != null)
                    bidsBuilder.Remove(orderToRemove);
                bids = bidsBuilder.ToImmutableSortedSet(comparer: OrderComparer.DescBidComparer());
            }
            else
            {
                var asksBuilder = asks.ToBuilder();
                var orderToRemove = asksBuilder.SingleOrDefault(o => o.ID == order.ID);
                if (orderToRemove != null)
                    asksBuilder.Remove(orderToRemove);
                asks = asksBuilder.ToImmutableSortedSet(comparer: OrderComparer.DescAskComparer());
            }
            #endregion

            #region update the status of the order in the dictionnary containing all the orders
            var ordersBuilder = orders.ToBuilder();

            ExchangeOrder changingOrder = null;
            ordersBuilder.TryGetValue(order.ID, out changingOrder);

            if (changingOrder != null)
                changingOrder = changingOrder.WithUpdatedStatus(ExchangeOrderStatus.Canceled);

            ordersBuilder[changingOrder.ID] = changingOrder;

            orders = ordersBuilder.ToImmutableDictionary();
            #endregion

            MockOrderBook result = new MockOrderBook(this.ExchangeID, orders, bids, asks);

            return result;


        }

        public override string ToString()
        {
            string res = "\n";
            res += string.Format("\n{0,-27}    {1, -27}\n", "===BIDS===", "===ASKS===");
            res += string.Format("\n{0,-8} {1,-7} {2, -10} || {0,-8} {1,-7} {2,-10}\n", "Exchange", "Price", "Amount", "");

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
}
