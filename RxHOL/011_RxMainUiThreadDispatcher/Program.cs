using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace _011_RxMainUiThreadDispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            TextBox t1 = new TextBox();
            Label l1 = new Label { Left = t1.Width + 20 };
            Form f1 = new Form
            {
                Controls = { t1, l1 }
            };

            var source = Observable
                .FromEventPattern(t1, "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(x=>(x.Sender as TextBox).Text)
                .DistinctUntilChanged();



            using (source
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(x => l1.Text = x))
            {
                Application.Run(f1);
            }

            

        }
    }
}
