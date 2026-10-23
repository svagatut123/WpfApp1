using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public static class AppTheme
    {
        public static bool IsDarkTheme { get; private set; } = false;

        public static void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ApplyTheme(IsDarkTheme);
        }

        public static void ApplyTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            var mainWindow = Application.Current.MainWindow;
            var resources = Application.Current.Resources;

            if (isDark)
            {
                Color darkBg = (Color)ColorConverter.ConvertFromString("#193D57");
                Color darkCard = (Color)ColorConverter.ConvertFromString("#256569");

                if (mainWindow != null) mainWindow.Background = new SolidColorBrush(darkBg);

                resources["WindowBackground"] = new SolidColorBrush(darkBg);
                resources["ContentBackground"] = new SolidColorBrush(darkCard);
                resources["TextColor"] = new SolidColorBrush(Colors.White);
            }
            else
            {
                Color lightBg = (Color)ColorConverter.ConvertFromString("#F3F3F3");
                Color lightCard = Colors.White;
                Color darkText = (Color)ColorConverter.ConvertFromString("#233E76");

                if (mainWindow != null) mainWindow.Background = new SolidColorBrush(lightBg);

                resources["WindowBackground"] = new SolidColorBrush(lightBg);
                resources["ContentBackground"] = new SolidColorBrush(lightCard);
                resources["TextColor"] = new SolidColorBrush(darkText);
            }
        }
    }
}