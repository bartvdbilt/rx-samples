using System;
using System.Linq;

using _012_RxUsageOfWCFServices.DictonarySuggestService;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace _012_RxUsageOfWCFServices
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new DictServiceSoapClient("DictServiceSoap");
            var match = Observable
                .FromAsyncPattern<string, string, string, DictionaryWord[]>(
                    service.BeginMatchInDict, service.EndMatchInDict);

            Func<string, IObservable<DictionaryWord[]>> matchInWordNetByPrefix = term => match("wn", term, "prefix");

            TextBox t1 = new TextBox();
            Form f1 = new Form
            {
                Controls = { t1 }
            };


            var textSource = Observable
                .FromEventPattern<EventArgs>(t1, "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(x => (x.Sender as TextBox).Text)
                .Where(x => x.Length > 0)
                .DistinctUntilChanged();

            var result = matchInWordNetByPrefix("react");

            var formRequest = textSource
                .Subscribe(x =>
                    {
                        var servicequestion = matchInWordNetByPrefix(x)
                            .Subscribe(words =>
                                {
                                    Console.WriteLine("{0} - {1}", x, words.Count());
                                    foreach (var w in words)
                                    {
                                        Console.Write("{0},", w.Word);
                                    }
                                    Console.WriteLine("\n*******************************************");
                                },
                                ex =>
                                {
                                    Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                });
                    });


            using (formRequest)
            {
                Application.Run(f1);
            }
        }


        void old()
        {
            var svc = new DictServiceSoapClient("DictServiceSoap");

            svc.BeginMatchInDict("wn", "react", "prefix",
                iar =>
                {
                    var words = svc.EndMatchInDict(iar);
                    foreach (var w in words)
                    {
                        Console.WriteLine(w.Word);
                    }
                }, null);

            Console.ReadLine();
        }
    }
}
