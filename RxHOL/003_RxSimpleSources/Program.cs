using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _003_RxSimpleSources
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable.Return(42);

            var subsscriber = source.Subscribe(
                x=>Console.WriteLine("Value: {0}",x),
                ex=>Console.WriteLine("Exception: {0}", ex.Message),
                ()=>Console.WriteLine("end!")
                );

            Console.WriteLine("ENTER to dispose");
            subsscriber.Dispose();
        }
    }
}
