using System.Windows;

namespace RestaurantApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Task.Run(() =>
            {
                using (var db = new AppDbContext())
                {
                    db.Database.CanConnect();
                }
            });

            var settings = RestaurantApp.Properties.Settings.Default;
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Themes/{settings.Theme}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[1] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Icons/{settings.Icons}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[2] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Localization/{settings.Language}", UriKind.Absolute) };
        }
    }

}
