using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var query = Core.Context.Content.AsQueryable();

            string search = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Title.ToLower().Contains(search) || x.Genre.ToLower().Contains(search));
            }

            if (ComboSort.SelectedItem is ComboBoxItem item)
            {
                string sort = item.Content.ToString();
                if (sort == "Сначала новые") query = query.OrderByDescending(x => x.ReleaseYear);
                else if (sort == "Сначала старые") query = query.OrderBy(x => x.ReleaseYear);
            }

            DataGridContent.ItemsSource = query.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadData();
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadData();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditContentPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridContent.SelectedItem as Content;
            if (selected == null)
            {
                MessageBox.Show("Выберите элемент для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Manager.MainFrame.Navigate(new AddEditContentPage(selected));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DataGridContent.SelectedItems.Cast<Content>().ToList();
            if (selectedList.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить выбранные записи ({selectedList.Count} шт.)?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Core.Context.Content.RemoveRange(selectedList);
                    Core.Context.SaveChanges();
                    LoadData();
                    MessageBox.Show("Успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChart_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChartPage());
        }

        // ПЕРЕХОД НА СТРАНИЦУ СМЕНЫ ПАРОЛЯ
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage(Core.LoggedUser));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из системы?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Manager.MainFrame.GoBack();
            }
        }
    }
}