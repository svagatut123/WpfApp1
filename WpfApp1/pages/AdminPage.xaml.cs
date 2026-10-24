using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

// Подключение сторонних библиотек для генерации отчетов
using OfficeOpenXml;             // NuGet: EPPlus
using Xceed.Words.NET;           // NuGet: DocX
using iTextSharp.text;           // NuGet: iTextSharp
using iTextSharp.text.pdf;       // NuGet: iTextSharp

namespace WpfApp1.pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        // Загрузка, фильтрация, поиск и сортировка данных
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
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new UsersManagementPage());
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            AppTheme.ToggleTheme();
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DataGridContent.SelectedItems.Cast<Content>().ToList();
            if (selectedList.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить выбранные записи ({selectedList.Count} шт.)?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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


        // 1. Экспорт в Excel 
        private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                FileName = $"Отчет_Контент_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();

                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Каталог контента");

                        worksheet.Cells["A1"].Value = "ID";
                        worksheet.Cells["B1"].Value = "Название";
                        worksheet.Cells["C1"].Value = "Тип";
                        worksheet.Cells["D1"].Value = "Жанр";
                        worksheet.Cells["E1"].Value = "Длительность (мин)";
                        worksheet.Cells["F1"].Value = "Год выпуска";
                        worksheet.Cells["G1"].Value = "Описание";

                        using (var range = worksheet.Cells["A1:G1"])
                        {
                            range.Style.Font.Bold = true;
                        }

                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cells[row, 1].Value = item.ContentID;
                            worksheet.Cells[row, 2].Value = item.Title;
                            worksheet.Cells[row, 3].Value = item.ContentType;
                            worksheet.Cells[row, 4].Value = item.Genre;
                            worksheet.Cells[row, 5].Value = item.DurationMinutes;
                            worksheet.Cells[row, 6].Value = item.ReleaseYear;
                            worksheet.Cells[row, 7].Value = item.Description;
                            row++;
                        }

                        worksheet.Cells.AutoFitColumns();

                        FileInfo fileInfo = new FileInfo(sfd.FileName);
                        package.SaveAs(fileInfo);
                    }

                    MessageBox.Show("Отчет Excel успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        // 2. Экспорт в  Word 
        private void BtnExportDocx_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = $"Отчет_Стриминг_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();

                    using (DocX doc = DocX.Create(sfd.FileName))
                    {
                        // Заголовок документа
                        var titleParagraph = doc.InsertParagraph("ОТЧЕТ ПО МЕДИАКОНТЕНТУ ПЛАТФОРМЫ");

                        titleParagraph.Font("Arial").FontSize(18).Bold()
                            .Color(Xceed.Drawing.Color.DarkCyan)
                            .Alignment = Xceed.Document.NET.Alignment.center;

                        // Дата составления отчета
                        var dateParagraph = doc.InsertParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        dateParagraph.Font("Arial").FontSize(11).Italic()
                            .Alignment = Xceed.Document.NET.Alignment.center;
                        doc.InsertParagraph().SpacingAfter(15);

                        // Создаем таблицу
                        Xceed.Document.NET.Table table = doc.AddTable(items.Count + 1, 7);
                        table.Alignment = Xceed.Document.NET.Alignment.center;

                        // Заполнение шапки через индексаторы коллекций Rows и Cells
                        var headerRow = table.Rows[0];
                        headerRow.Cells[0].Paragraphs.First().Append("ID").Bold();
                        headerRow.Cells[1].Paragraphs.First().Append("Название").Bold();
                        headerRow.Cells[2].Paragraphs.First().Append("Тип").Bold();
                        headerRow.Cells[3].Paragraphs.First().Append("Жанр").Bold();
                        headerRow.Cells[4].Paragraphs.First().Append("Длит.").Bold();
                        headerRow.Cells[5].Paragraphs.First().Append("Год").Bold();
                        headerRow.Cells[6].Paragraphs.First().Append("Описание").Bold();

                        // Построчное наполнение таблицы с прямой адресацией по индексам ячеек [0..6]
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            var dataRow = table.Rows[i + 1];

                            dataRow.Cells[0].Paragraphs.First().Append(item.ContentID.ToString());
                            dataRow.Cells[1].Paragraphs.First().Append(item.Title ?? "");
                            dataRow.Cells[2].Paragraphs.First().Append(item.ContentType ?? "");
                            dataRow.Cells[3].Paragraphs.First().Append(item.Genre ?? "");
                            dataRow.Cells[4].Paragraphs.First().Append($"{item.DurationMinutes} мин.");
                            dataRow.Cells[5].Paragraphs.First().Append(item.ReleaseYear.ToString());
                            dataRow.Cells[6].Paragraphs.First().Append(item.Description ?? "");
                        }

                        doc.InsertTable(table);
                        doc.Save();
                    }

                    MessageBox.Show("Отчет Word успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 3. Экспорт в PDF
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Документ PDF (.pdf)|.pdf",
                FileName = $"Отчет_Контент_{DateTime.Now:yyyyMMdd}"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var items = Core.Context.Content.ToList();
                    // Используем полное имя класса iTextSharp.text.Document, чтобы исключить конфликты
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4, 20, 20, 20, 20);
                    PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
                    doc.Open();
                    // Настройка шрифтов для корректного вывода русского текста
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf");
                    BaseFont bfArial = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font titleFont = new iTextSharp.text.Font(bfArial, 18, iTextSharp.text.Font.BOLD, new BaseColor(1, 152, 175));
                    iTextSharp.text.Font regularFont = new iTextSharp.text.Font(bfArial, 9, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font boldFont = new iTextSharp.text.Font(bfArial, 9, iTextSharp.text.Font.BOLD);
                    // Главный заголовок
                    Paragraph pTitle = new Paragraph("ОТЧЕТ СТРИМИНГОВОЙ ПЛАТФОРМЫ", titleFont);
                    pTitle.Alignment = Element.ALIGN_CENTER;
                    doc.Add(pTitle);
                    // Дата формирования
                    Paragraph pDate = new Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n", regularFont);
                    pDate.Alignment = Element.ALIGN_CENTER;
                    doc.Add(pDate);
                    // Сетка таблицы PDF на 7 колонок
                    PdfPTable table = new PdfPTable(7);
                    table.WidthPercentage = 100;
                    float[] widths = new float[] { 5f, 20f, 10f, 12f, 8f, 7f, 38f };
                    table.SetWidths(widths);
                    table.AddCell(new PdfPCell(new Phrase("ID", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Название", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Тип", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Жанр", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Длит.", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Год", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Описание", boldFont)));
                    foreach (var item in items)
                    {
                        table.AddCell(new PdfPCell(new Phrase(item.ContentID.ToString(), regularFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Title ?? "", regularFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.ContentType ?? "", regularFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Genre ?? "", regularFont)));
                        table.AddCell(new PdfPCell(new Phrase($"{item.DurationMinutes} м.", regularFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.ReleaseYear.ToString(), regularFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Description ?? "", regularFont)));
                    }
                    doc.Add(table);
                    doc.Close();
                    MessageBox.Show("Отчет PDF успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
