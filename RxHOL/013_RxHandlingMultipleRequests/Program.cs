using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using _013_RxHandlingMultipleRequests.DSS;

namespace _013_RxHandlingMultipleRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new TextBox();
            var l1 = new ListBox { Top = t1.Height + 10, Height = 250, Width = 150 };
            var f1 = new Form
            {
                Controls = { t1, l1 }
            };

            var textSource = Observable
                .FromEventPattern<EventArgs>(t1, "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Select(x => (x.Sender as TextBox).Text)
                .Where(x => x.Length >= 3)
                .DistinctUntilChanged()
                .Do(Console.WriteLine);

            var service = new DictServiceSoapClient("DictServiceSoap");
            var dictSource = Observable
                .FromAsyncPattern<string, string, string, DictionaryWord[]>(service.BeginMatchInDict, service.EndMatchInDict);

            Func<string, IObservable<DictionaryWord[]>> matchInWordNetByPrefix = term => dictSource("wn", term, "prefix");


            //var res = from term in textSource
            //          from words in matchInWordNetByPrefix(term)
            //                          .Finally(() => Console.WriteLine("Disposed request for: " + term))
            //                          .TakeUntil(textSource)
            //          select words;

            var res = (from term in textSource
                       select matchInWordNetByPrefix(term))
                       .Switch();
                      

            using (res
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(w =>
                {
                    l1.Items.Clear();
                    l1.Items.AddRange(w.Select(word => word.Word).ToArray());
                },
                ex =>
                {
                    MessageBox.Show(ex.Message);
                }))

                Application.Run(f1);
        }
    }
}
