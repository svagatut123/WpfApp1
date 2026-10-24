using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class LoginPage : Page
    {
        private int failedAttempts = 0;
        private string generatedCaptcha = "";

        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = PassBox.Password.Trim();

            // Проверка капчи, если было 3 или более неудачных попыток
            if (BlockCaptcha.Visibility == Visibility.Visible)
            {
                if (TxtInputCaptcha.Text.Trim() != generatedCaptcha)
                {
                    MessageBox.Show("Неверная капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GenerateCaptcha();
                    return;
                }
            }

            // Поиск пользователя в БД
            var user = Core.Context.Users.FirstOrDefault(x => x.Login == login && x.Password == password);

            if (user != null)
            {
                // Сбрасываем счетчик неудачных попыток при успешном входе
                failedAttempts = 0;
                BlockCaptcha.Visibility = Visibility.Collapsed;

                Core.LoggedUser = user;

                MessageBox.Show($"Добро пожаловать, {user.FullName}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переход по ролям (RoleID: 1 - Администратор, 2 - Пользователь)
                if (user.RoleID == 1)
                {
                    Manager.MainFrame.Navigate(new AdminPage());
                }
                else
                {
                    Manager.MainFrame.Navigate(new UserPage());
                }
            }
            else
            {
                failedAttempts++;
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Если 3 неудачные попытки — показываем блок капчи
                if (failedAttempts >= 3)
                {
                    BlockCaptcha.Visibility = Visibility.Visible;
                    GenerateCaptcha();
                }
            }
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            AppTheme.ToggleTheme();
        }

        private void GenerateCaptcha()
        {
            string chars = "ABCDFGHJKLMNPQRSTUVWXYZ0123456789";
            Random random = new Random();
            generatedCaptcha = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            TxtCaptcha.Text = generatedCaptcha;
            TxtInputCaptcha.Clear();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new RegisterPage());
        }
    }
}