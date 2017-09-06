using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _001_RxIsIDisposable
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable.Empty<int>();
            IObserver<int> handler = null;

            IDisposable subscription = source.Subscribe();
            Console.WriteLine("Press ENTER to unsubscribe and dispose");
            Console.ReadLine();

            subscription.Dispose();
        }
    }
}
