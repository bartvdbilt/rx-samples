using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace _009_RxFromWinFormToConsoleWithDataProjections
{
    class Program
    {
        static void Main(string[] args)
        {
            var textbox = new TextBox();
            var form = new Form
            {
                Controls = { textbox }
            };

            var moves = Observable
                .FromEventPattern<MouseEventArgs>(form, "MouseMove")
                .Select(e => e.EventArgs.Location);

            var texts = Observable
                .FromEventPattern<EventArgs>(textbox, "TextChanged")
                .Select(e => (e.Sender as TextBox).Text);

            
            
            
            var msubs = moves.Subscribe(x => Console.WriteLine("mouse position: {0}", x));

            var ksubs = texts.Subscribe(x => Console.WriteLine("Textbox text: {0}", x));





            using (new CompositeDisposable(msubs, ksubs))
            {
                Application.Run(form);
            }
        }
    }
}
