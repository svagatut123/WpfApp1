using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Передаем фрейм в менеджер навигации
            Manager.MainFrame = MainFrame;

            // Стартовая страница
            MainFrame.Navigate(new pages.LoginPage());
        }

        // Обработчик динамического изменения заголовка (Пункт 5)
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page && !string.IsNullOrEmpty(page.Title))
            {
                this.Title = $"Стриминговая платформа — {page.Title}";
            }
        }
    }
}