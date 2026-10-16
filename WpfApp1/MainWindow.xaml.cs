using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Manager.MainFrame = MainFrame;
            MainFrame.Navigate(new pages.LoginPage());
        }
    }

    public static class Manager
    {
        public static Frame MainFrame { get; set; }
    }
}