using OrderBookAggregator.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderBookAggregator.WPF
{
    public class ExchangeViewModel : ViewModelBase
    {
        public ExchangeViewModel(IExchangeClient exchangeClient)
        {
            UnderlyingExchange = exchangeClient;

            orderStreamSubscription = UnderlyingExchange.OrderStream
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOnDispatcher()
                    .Subscribe((order) =>
                    {
                        Orders.Add(order);
                    });

            orderBookStreamSubscription = UnderlyingExchange.OrderBookStream
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe((ob) =>
                {
                    Bids = ob.Bids.ToList();
                    Asks = ob.Asks.ToList();
                });
        }

        IDisposable orderStreamSubscription;
        IDisposable orderBookStreamSubscription;

        IExchangeClient underlyingExchange;
        public IExchangeClient UnderlyingExchange
        {
            get
            {
                return underlyingExchange;
            }
            set
            {
                underlyingExchange = value;
                RaisePropertyChanged(() => this.UnderlyingExchange);
            }
        }

        ObservableCollection<ExchangeOrder> orders;
        public ObservableCollection<ExchangeOrder> Orders
        {
            get
            {
                if (orders == null)
                    orders = new ObservableCollection<ExchangeOrder>();
                return orders;
            }
            set
            {
                orders = value;
                RaisePropertyChanged(() => this.Orders);
            }
        }

        List<ExchangeOrder> bids;
        public List<ExchangeOrder> Bids
        {
            get
            {
                if (bids == null)
                    bids = new List<ExchangeOrder>();
                return bids;
            }
            set
            {
                if (value != bids)
                {
                    bids = value;
                    RaisePropertyChanged(() => this.Bids);
                }
            }
        }

        List<ExchangeOrder> asks;
        public List<ExchangeOrder> Asks
        {
            get
            {
                if (asks == null)
                    asks = new List<ExchangeOrder>();
                return asks;
            }
            set
            {
                if (value != asks)
                {
                    asks = value;
                    RaisePropertyChanged(() => this.Asks);
                }
            }
        }

        public string Name 
        {
            get
            {
                return string.Format("Exchange {0}", UnderlyingExchange.ExchangeID);
            }
        }

        bool? busy;
        public bool Busy
        {
            get
            {
                return busy ?? false;
            }
            set
            {
                busy = value;
                RaisePropertyChanged(() => this.Busy);
            }
        }

        RelayCommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                {
                    _startCommand = new RelayCommand(param => this.Start(), param => !this.Busy);
                }
                return _startCommand;
            }
        }

        RelayCommand _stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                {
                    _stopCommand = new RelayCommand(param => this.Stop(), param => this.Busy);
                }
                return _stopCommand;
            }
        }

        public void Start()
        {

            UnderlyingExchange.Connect();
            if (UnderlyingExchange.GetType() == typeof(MockClient))
            {
                ((MockClient)UnderlyingExchange).StartRandomOrders();
            }

            Busy = true;
        }

        //Stops the underlying hot stream (by disposing the IDisposable returned by the Connect() call on the OrderFactory's random orders stream)
        //This way, all the subscriptions which are 'downstream' (like the ones in the MainWindowViewModel) will also stop receiving messages
        public void Stop()
        {

            UnderlyingExchange.Disconnect();
            if (UnderlyingExchange.GetType() == typeof(MockClient))
            {
                ((MockClient)UnderlyingExchange).StopRandomOrders();
            }

            Busy = false;
        }
    }
}
