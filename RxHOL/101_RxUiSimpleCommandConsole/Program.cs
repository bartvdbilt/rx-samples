using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI.Xaml;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace _101_RxUiSimpleCommandConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application();
            ThreadPool.QueueUserWorkItem(_ =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                        {
                            var cmd = ReactiveCommand.Create(x => true, x => Console.WriteLine("X"));
                            cmd.Execute(42);
                        }), null);                        
                }, null);

            Thread.Yield();
            Console.WriteLine("ENTER");
            Console.ReadLine();
        }
    }
}
