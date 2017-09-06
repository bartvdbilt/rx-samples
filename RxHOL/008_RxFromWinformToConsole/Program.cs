using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace _008_RxFromWinformToConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var textbox = new TextBox();
            var form = new Form
            {
                Controls = { textbox}
            };

            var moves = Observable.FromEventPattern<MouseEventArgs>(form, "MouseMove");
            var texts = Observable.FromEventPattern<EventArgs>(textbox, "TextChanged");

            var msubs = moves.Subscribe(
                x => Console.WriteLine("mouse position: {0}", x.EventArgs.Location.ToString())
                );

            var ksubs = texts.Subscribe(
                x=>Console.WriteLine("Textbox text: {0}", (x.Sender as TextBox).Text)
                );

            using (new CompositeDisposable(msubs, ksubs))
            {
                Application.Run(form);
            }
            

        }
    }
}
