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

namespace _100_RxUiSimpleCommand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ReactiveCommand FirstRxCommand { get; set; }
        public MainWindow()
        {
            DataContext = this;
            this.InitializeCommands();
            InitializeComponent();
        }

        private void InitializeCommands()
        {
            this.FirstRxCommand = ReactiveCommand.Create(x => true, x =>
                {
                    Console.WriteLine("Teraz z konsoli");
                });
            

            this.FirstRxCommand.Subscribe(x => MessageBox.Show("From subscription"));
        }
    }
}
