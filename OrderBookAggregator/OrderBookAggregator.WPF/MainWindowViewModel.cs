using OrderBookAggregator.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.WPF
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {

            //Create ExchangeClients
            var mockExchangeClient0 = new MockClient(0);
            var mockExchangeClient1 = new MockClient(1);
            var mockExchangeClient2 = new MockClient(2);

            //store them in an array
            var exchanges = new IExchangeClient[3] { mockExchangeClient0, mockExchangeClient1, mockExchangeClient2 };

            //instantiate the corresponding ViewModels
            //these controls allow to connect and disconnect from the underlying exchanges at will.
            ExchangeVMs = exchanges.Select(e => new ExchangeViewModel(e)).ToList();

            //merge their orderbook streams 
            var simpleOrderBookStream = Observable.Merge(ExchangeVMs.Select(e => e.UnderlyingExchange.OrderBookStream));

            //scan the resulting stream into a stream of aggregated orderbooks (orderbooks with orders coming from different exchanges)
            var aggregatedOrderBookStream = simpleOrderBookStream.Scan(new AggregatedOrderBook(), (aob, ob) => aob.InsertBook(ob));


            //subscribe to the aggregatedOrderBookStream. 
            //each time a new aggregated orderbook is received, the list of bids and asks is updated in the UI.
            //ATTENTION: no elements will be observed until at leat one individual exchange is connected to
            aggregateStreamSubscription = aggregatedOrderBookStream.SubscribeOn(NewThreadScheduler.Default)
                                                .ObserveOnDispatcher()
                                                .Subscribe((aob) =>
                                                {
                                                    Bids = aob.Bids.ToList();
                                                    Asks = aob.Asks.ToList();
                                                });

        }

        IDisposable aggregateStreamSubscription;
        
        List<ExchangeViewModel> exchangeVMs;
        public List<ExchangeViewModel> ExchangeVMs
        {
            get
            {
                return exchangeVMs;
            }
            set
            {
                exchangeVMs = value;
                RaisePropertyChanged(() => this.ExchangeVMs);
            }
        }

        List<ExchangeOrder> bids;
        public List<ExchangeOrder> Bids
        {
            get
            {
                if (bids == null)
                {
                    bids = new List<ExchangeOrder>();
                }
                return bids;
            }
            set
            {
                if (value != bids)
                {
                    bids = value;
                    this.RaisePropertyChanged(() => this.Bids);
                }
            }
        }

        List<ExchangeOrder> asks;
        public List<ExchangeOrder> Asks
        {
            get
            {
                if (asks == null)
                {
                    asks = new List<ExchangeOrder>();
                }
                return asks;
            }
            set
            {
                if (value != asks)
                {
                    asks = value;
                    this.RaisePropertyChanged(() => this.Asks);
                }
            }
        }
    }
}
