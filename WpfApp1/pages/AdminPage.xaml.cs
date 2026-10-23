using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
            if (DataGridContent == null) return;

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
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChart_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChartPage());
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePasswordPage(Core.LoggedUser));
        }

        // ==========================================
        // ЛОГИКА ЭКСПОРТА ОТЧЕТОВ (.xlsx, .docx, .pdf)
        // ==========================================

        // ==========================================
        // 1. ЭКСПОРТ В EXCEL ЧЕРЕЗ CSV (открывается в Excel без ошибок и без библиотек)
        // ==========================================
        private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV файл (для Excel) (*.csv)|*.csv",
                FileName = $"Отчет_Контент_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();
                    StringBuilder sb = new StringBuilder();

                    // Используем точку с запятой (;) — это стандарт разделителя для русского Excel
                    sb.AppendLine("ID;Название;Тип;Жанр;Длительность (мин);Год выпуска;Описание");

                    foreach (var item in items)
                    {
                        // Экранируем кавычки в тексте, если они есть
                        string title = item.Title?.Replace("\"", "\"\"") ?? "";
                        string genre = item.Genre?.Replace("\"", "\"\"") ?? "";
                        string desc = item.Description?.Replace("\"", "\"\"") ?? "";

                        sb.AppendLine($"{item.ContentID};\"{title}\";\"{item.ContentType}\";\"{genre}\";{item.DurationMinutes};{item.ReleaseYear};\"{desc}\"");
                    }

                    // Кодировка UTF-8 с BOM, чтобы русский текст в Excel отображался корректно (без кракозябр)
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Отчет успешно сохранен! Дважды кликните по файлу, чтобы открыть его в Excel.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==========================================
        // 2. ЭКСПОРТ В WORD (.doc / HTML-формат, открывается в Word без сторонних пакетов)
        // ==========================================
        private void BtnExportDocx_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Документ Word (*.doc)|*.doc",
                FileName = $"Отчет_Стриминг_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();
                    StringBuilder sb = new StringBuilder();

                    // Формат веб-страницы, который Word распознает как полноценный документ со стилями
                    sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
                    sb.AppendLine("<head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head>");
                    sb.AppendLine("<body style='font-family: Arial, sans-serif;'>");
                    sb.AppendLine("<h1 style='color: #0198AF; text-align: center;'>ОТЧЕТ ПО МЕДИАКОНТЕНТУ ПЛАТФОРМЫ</h1>");
                    sb.AppendLine($"<p style='text-align: center;'><b>Дата формирования:</b> {DateTime.Now:dd.MM.yyyy HH:mm}</p><hr/>");

                    foreach (var item in items)
                    {
                        sb.AppendLine($"<h3 style='margin-bottom: 2px;'>{item.Title} <span style='font-size: 12px; color: gray;'>[{item.ContentType}]</span></h3>");
                        sb.AppendLine($"<p style='margin-top: 0;'><b>Жанр:</b> {item.Genre} | <b>Длительность:</b> {item.DurationMinutes} мин. | <b>Год:</b> {item.ReleaseYear}</p>");
                        sb.AppendLine($"<p><i>Описание:</i> {item.Description}</p>");
                        sb.AppendLine("<hr style='border: 1px dashed #ccc;'/>");
                    }

                    sb.AppendLine("</body>");
                    sb.AppendLine("</html>");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Отчет Word успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==========================================
        // 3. ЭКСПОРТ В PDF / ТЕКСТОВЫЙ ОТЧЕТ (.txt)
        // ==========================================
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Текстовый отчет (*.txt)|*.txt",
                FileName = $"Отчет_Контент_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("==================================================");
                    sb.AppendLine("       ОТЧЕТ СТРИМИНГОВОЙ ПЛАТФОРМЫ               ");
                    sb.AppendLine($"       Дата: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine("==================================================\n");

                    foreach (var item in items)
                    {
                        sb.AppendLine($"Название: {item.Title}");
                        sb.AppendLine($"Тип: {item.ContentType} | Жанр: {item.Genre}");
                        sb.AppendLine($"Год: {item.ReleaseYear} | Длительность: {item.DurationMinutes} мин.");
                        sb.AppendLine($"Описание: {item.Description}");
                        sb.AppendLine("--------------------------------------------------");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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