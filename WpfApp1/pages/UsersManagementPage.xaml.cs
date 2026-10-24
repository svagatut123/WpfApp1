using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class UsersManagementPage : Page
    {
        public UsersManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (DataGridUsers == null) return;

            var query = Core.Context.Users.AsQueryable();

            string search = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Login.ToLower().Contains(search) || x.FullName.ToLower().Contains(search));
            }

            DataGridUsers.ItemsSource = query.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadUsers();

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = DataGridUsers.SelectedItem as Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Manager.MainFrame.Navigate(new AddEditUserPage(selectedUser));
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            AppTheme.ToggleTheme();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsers = DataGridUsers.SelectedItems.Cast<Users>().ToList();
            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить выбранных пользователей ({selectedUsers.Count} шт.)?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Core.Context.Users.RemoveRange(selectedUsers);
                    Core.Context.SaveChanges();
                    LoadUsers();
                    MessageBox.Show("Успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
