using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace _006_RxGenerateWithTime
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = Observable.Generate(
                0,
                i => i < 10,
                i => i + 1,
                i => i * i,
                i => TimeSpan.FromSeconds(i)
                );

            using (var s = source.Subscribe(
                x => Console.WriteLine("next: {0}", x),
                ex => Console.WriteLine("exception: {0}", ex.Message),
                () => Console.WriteLine("no more")))
            {
                Console.WriteLine("ENTER");
                Console.ReadLine();
            }



        }
    }
}
