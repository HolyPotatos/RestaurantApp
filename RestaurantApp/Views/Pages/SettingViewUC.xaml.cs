using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingViewUC.xaml
    /// </summary>
    public partial class SettingViewUC : UserControl
    {
        public SettingViewUC()
        {
            InitializeComponent();
        }

        private bool _isInitializing = true;

        private void SettingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_isInitializing) return; 
            RestaurantApp.Properties.Settings.Default.Theme = ThemeCB.SelectedIndex == 0 ? "LightTheme.xaml" : "DarkTheme.xaml";
            RestaurantApp.Properties.Settings.Default.Icons = IconsCB.SelectedIndex == 0 ? "MaterialDesignIconsDictionary.xaml" : "LucideIconsDictionary.xaml";
            RestaurantApp.Properties.Settings.Default.Language = LanguageCB.SelectedIndex == 0 ? "ru-RU.xaml" : "en-US.xaml";
            RestaurantApp.Properties.Settings.Default.Save();
            

            var settings = RestaurantApp.Properties.Settings.Default;
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Themes/{settings.Theme}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[1] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Icons/{settings.Icons}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[2] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Localization/{settings.Language}", UriKind.Absolute) };

        }

        private void UCLoaded(object sender, RoutedEventArgs e)
        {
            var settings = RestaurantApp.Properties.Settings.Default;
            ThemeCB.SelectedIndex = settings.Theme == "LightTheme.xaml" ? 0 : 1;
            IconsCB.SelectedIndex = settings.Icons == "MaterialDesignIconsDictionary.xaml" ? 0 : 1;
            LanguageCB.SelectedIndex = settings.Language == "ru-RU.xaml" ? 0 : 1;
            _isInitializing = false;
        }
    }
}
