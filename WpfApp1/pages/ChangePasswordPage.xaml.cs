using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class ChangePasswordPage : Page
    {
        private Users currentUser;

        // В конструктор передаем объект текущего авторизованного пользователя
        public ChangePasswordPage(Users user)
        {
            InitializeComponent();
            currentUser = user;
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = PassOld.Password.Trim();
            string newPass = PassNew.Password.Trim();
            string confirmPass = PassConfirm.Password.Trim();

            // Проверка правильности старого пароля
            if (currentUser.Password != oldPass)
            {
                MessageBox.Show("Введен неверный старый пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на пустоту нового пароля
            if (string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Новый пароль не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка совпадения нового пароля и подтверждения
            if (newPass != confirmPass)
            {
                MessageBox.Show("Новые пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Обновляем пароль в базе данных через Entity Framework
            var userInDb = Core.Context.Users.FirstOrDefault(x => x.UserID == currentUser.UserID);
            if (userInDb != null)
            {
                userInDb.Password = newPass;
                Core.Context.SaveChanges();

                MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}