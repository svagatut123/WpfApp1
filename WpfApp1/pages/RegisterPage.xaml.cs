using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string login = TxtLogin.Text.Trim();
            string password = PassBox.Password.Trim();
            string confirmPassword = PassBoxConfirm.Password.Trim();

            // Проверка на заполненность полей
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на существование логина
            if (Core.Context.Users.Any(x => x.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание нового пользователя (RoleID = 2 по умолчанию роли "Пользователь")
            var newUser = new Users
            {
                FullName = fullName,
                Login = login,
                Password = password, // В реальном проекте здесь применяется хэширование (например, SHA256)
                RoleID = 2
            };

            try
            {
                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();
                MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}