using System;
using System.Windows;

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
            string themeUri = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";

            var newDict = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newDict);
        }
    }
}
