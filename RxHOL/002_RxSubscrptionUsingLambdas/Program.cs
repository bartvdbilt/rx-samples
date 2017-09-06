using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _002_RxSubscrptionUsingLambdas
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable.Empty<int>();

            IDisposable subscription = source.Subscribe(
                x=> Console.WriteLine("Has new value {0}", x),
                ex=>Console.WriteLine("Exception cought {0}", ex.Message),
                ()=>Console.WriteLine("No more items")
                );

            Console.WriteLine("ENTER to dispose");
            subscription.Dispose();
        }
    }
}
