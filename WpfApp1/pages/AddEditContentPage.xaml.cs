using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.pages
{
    public partial class AddEditContentPage : Page
    {
        private Content currentContent = null;

        public AddEditContentPage(Content content)
        {
            InitializeComponent();
            if (content != null)
            {
                currentContent = content;
                TxtTitleHeader.Text = "Редактирование медиаконтента";
                TxtTitle.Text = currentContent.Title;
                ComboType.Text = currentContent.ContentType;
                TxtGenre.Text = currentContent.Genre;
                TxtDuration.Text = currentContent.DurationMinutes.ToString();
                TxtYear.Text = currentContent.ReleaseYear.ToString();
                TxtDescription.Text = currentContent.Description;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtTitle.Text) || ComboType.SelectedItem == null || string.IsNullOrEmpty(TxtGenre.Text))
            {
                MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtDuration.Text, out int duration) || !int.TryParse(TxtYear.Text, out int year))
            {
                MessageBox.Show("Длительность и год должны быть числами!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (currentContent == null)
            {
                var newContent = new Content
                {
                    Title = TxtTitle.Text.Trim(),
                    ContentType = (ComboType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Genre = TTxtGenreText(), // исправлено ниже
                    DurationMinutes = duration,
                    ReleaseYear = year,
                    Description = TxtDescription.Text.Trim()
                };
                // Чтобы не было опечатки:
                newContent.Genre = TxtGenre.Text.Trim();
                Core.Context.Content.Add(newContent);
            }
            else
            {
                currentContent.Title = TxtTitle.Text.Trim();
                currentContent.ContentType = (ComboType.SelectedItem as ComboBoxItem)?.Content.ToString();
                currentContent.Genre = TxtGenre.Text.Trim();
                currentContent.DurationMinutes = duration;
                currentContent.ReleaseYear = year;
                currentContent.Description = TxtDescription.Text.Trim();
            }

            try
            {
                Core.Context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private string TTxtGenreText() => TxtGenre.Text.Trim();

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.GoBack();
    }
}