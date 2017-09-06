using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace _010_RxDistinctEvents
{
    class Timestamp
    {
        public void Main()
        {
            var textbox = new TextBox();
            var form = new Form
            {
                Controls = { textbox }
            };

            var moves = Observable
                .FromEventPattern<MouseEventArgs>(form, "MouseMove")
                .Select(e => e.EventArgs.Location)
                .Timestamp()
                .Do(x => Console.WriteLine("{0} - {1}", x.Timestamp.Millisecond, x.Value))
                .Select(x => x.Value)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Timestamp()
                .Do(x => Console.WriteLine("{0} - {1}", x.Timestamp.Millisecond, x.Value))
                .Select(x => x.Value)
                .DistinctUntilChanged();

            var keys = Observable
                .FromEventPattern<EventArgs>(textbox, "TextChanged")
                .Select(e => (e.Sender as TextBox).Text)
                .Timestamp()
                .Do(x => Console.WriteLine("{0} - {1}", x.Timestamp.Millisecond, x.Value))
                .Select(x => x.Value)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Timestamp()
                .Do(x => Console.WriteLine("{0} - {1}", x.Timestamp.Millisecond, x.Value))
                .Select(x => x.Value)
                .DistinctUntilChanged();




            var msubs = moves.Subscribe(x => Console.WriteLine("mouse position: {0}", x));

            var ksubs = keys.Subscribe(x=>Console.WriteLine("Typed: {0}", x));




            using (new CompositeDisposable(msubs, ksubs))
            {
                Application.Run(form);
            }
        }
    }
}
