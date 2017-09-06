using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace _007_RxWinFormMouseMove
{
    class Program
    {
        static void Main(string[] args)
        {
            // OLD WAY 
            //var label = new Label();
            //var form = new Form
            //{
            //    Controls = {label}
            //};

            //form.MouseMove += (s, e) =>
            //    {
            //        label.Text = e.Location.ToString();
            //    };

            //Application.Run(form);


            // NEW WAY WITH RX

            var label = new Label();
            var form = new Form
            {
                Controls = { label }
            };

            var moves = Observable.FromEventPattern<MouseEventArgs>(form, "MouseMove");

            using (moves
                .Subscribe(
                    x => label.Text = x.EventArgs.Location.ToString(),
                    ex => label.Text = ex.Message,
                    () => label.Text = "Mouse is over?!"
                           )
                  )
            {
                Application.Run(form);

            }
        }
    }
}
