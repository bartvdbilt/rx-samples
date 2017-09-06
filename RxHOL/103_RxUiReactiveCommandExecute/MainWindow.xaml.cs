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
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using System.ComponentModel;

namespace _103_RxUiReactiveCommandExecute
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ReactiveCommand RxCommand { get; set; }

        public int value;
        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.InitializeCommands();
            this.Value = 100;

            this.DataContext = this;
        }

        private void InitializeCommands()
        {
            this.RxCommand= ReactiveCommand.Create(x => x is int);

            this.RxCommand
                .Where(x => ((int)x) % 2 == 0)
                .Subscribe(x => Console.WriteLine("We're even!"));

            this.RxCommand
                .Where(x => ((int)x) % 2 != 0)
                .Subscribe(x => Console.WriteLine("That's odd"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
