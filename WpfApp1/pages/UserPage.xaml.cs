using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            if (DataGridContent == null) return;

            var query = Core.Context.Content.AsQueryable();

            // Поиск по названию или жанру
            string search = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Title.ToLower().Contains(search) || x.Genre.ToLower().Contains(search));
            }

            // Фильтрация по выбранному типу контента
            if (ComboTypeFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                string filterType = selectedItem.Content.ToString();
                if (filterType != "Все типы")
                {
                    query = query.Where(x => x.ContentType == filterType);
                }
            }

            DataGridContent.ItemsSource = query.ToList();
        }

        private void Filter_Changed(object sender, TextChangedEventArgs e) => LoadData();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadData();

        // Кнопка смены пароля (передает текущего залогиненного пользователя)
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage(Core.LoggedUser));
        }

        // Кнопка смены темы приложения
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            AppTheme.ToggleTheme();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Manager.MainFrame.GoBack();
            }
        }
    }
}