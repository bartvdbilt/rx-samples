using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using ReactiveUI.Xaml;

namespace _102_RxUiChainingReactiveCommands
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDisposable d;
        public ReactiveCommand cmd { get; set; }
        private IDisposable m;
        public MainWindow()
        {
            InitializeComponent();

            this.PlayWithRxCommands();

            this.DataContext = this;

        }

        private void PlayWithRxCommands()
        {
            //var mouse1 = Observable
            //    .FromEventPattern(this, "MouseDown")
            //    .Select(_ => false);

            //var mouse2 = Observable
            //    .FromEventPattern(this, "MouseUp")
            //    .Select(_ => true);

            //var mouseIsUp = Observable
            //    .Merge(new[]{mouse1, mouse2})
            //    .StartWith(true);

            //var cmd = new ReactiveCommand(mouseIsUp);
            //cmd.Subscribe(x => Console.WriteLine(x));

            var mouseIsUp = Observable
                .Merge(Observable
                        .FromEventPattern<MouseButtonEventArgs>(this, "MouseDown")
                            .Select(_ => false),
                       Observable
                        .FromEventPattern<MouseButtonEventArgs>(this, "MouseUp")
                            .Select(_ => true))
                .StartWith(true);

            this.m = mouseIsUp.Subscribe(x=>Console.WriteLine(x));

            this.cmd = new ReactiveCommand(mouseIsUp);
            this.d = this.cmd.Subscribe(x => Console.WriteLine(x));


        }
    }
}
