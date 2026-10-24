using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class AddEditUserPage : Page
    {
        private Users _currentUser;

        public AddEditUserPage(Users user)
        {
            InitializeComponent();

            _currentUser = user;

            // Заполняем поля текущими значениями
            TxtLogin.Text = _currentUser.Login;
            TxtFullName.Text = _currentUser.FullName;
            TxtRoleID.Text = _currentUser.RoleID.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentUser.Login = TxtLogin.Text.Trim();
                _currentUser.FullName = TxtFullName.Text.Trim();

                if (int.TryParse(TxtRoleID.Text, out int roleId))
                {
                    _currentUser.RoleID = roleId;
                }
                else
                {
                    MessageBox.Show("ID роли должен быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
