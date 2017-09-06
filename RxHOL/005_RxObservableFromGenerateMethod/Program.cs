using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _005_RxObservableFromGenerateMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable
                .Generate(0, // initial state
                i => i < 10, // condition
                i => i + 1,  // iteration step
                i => i * i); // iteration operation

            using (var s = source.Subscribe(
                x => Console.WriteLine(x)       // only the working stuff will be handled
                                                // no errors and no exceptions
                                                // no information about sequence finish either
                )) { };
        }
    }
}
