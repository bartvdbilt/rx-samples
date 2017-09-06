using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _004_RxObservableFromRangeOfInts
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable.Range(5, 7);

            using (var subs = source.Subscribe(
                x => Console.WriteLine(x),
                ex => Console.WriteLine("!{0}", ex.Message),
                () => Console.WriteLine("end!")
                )) { }

            Console.WriteLine("ENTER to dispose");

        }
    }
}
