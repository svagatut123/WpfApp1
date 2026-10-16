using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.pages
{
    public partial class ChartPage : Page
    {
        public ChartPage()
        {
            InitializeComponent();
            LoadChartData();
        }

        private void LoadChartData()
        {
            // Группируем контент из базы данных по типам (ContentType) и считаем количество записей
            var data = Core.Context.Content
                .GroupBy(c => c.ContentType)
                .Select(g => new
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Находим максимальное значение для пропорционального масштабирования столбцов (максимальная высота столбца = 200 пикселей)
            int maxCount = data.Count > 0 ? data.Max(x => x.Count) : 1;
            if (maxCount == 0) maxCount = 1;

            var chartItems = data.Select(x => new
            {
                x.TypeName,
                x.Count,
                BarHeight = (double)x.Count / maxCount * 200
            }).ToList();

            // Передаем подготовленные данные в графический элемент
            IcChart.ItemsSource = chartItems;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}