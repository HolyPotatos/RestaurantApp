using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingViewUC.xaml
    /// </summary>
    public partial class SettingViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        private int _currentUserID;
        private bool _isInitializing = true;
        public SettingViewUC(int currentUserID)
        {
            InitializeComponent();
            _currentUserID = currentUserID;
        }

        private void SettingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            Properties.Settings.Default.Theme = ThemeCB.SelectedIndex == 0 ? "LightTheme.xaml" : "DarkTheme.xaml";
            Properties.Settings.Default.Icons = IconsCB.SelectedIndex == 0 ? "MaterialDesignIconsDictionary.xaml" : "LucideIconsDictionary.xaml";
            Properties.Settings.Default.Language = LanguageCB.SelectedIndex == 0 ? "ru-RU.xaml" : "en-US.xaml";
            Properties.Settings.Default.Save();


            var settings = Properties.Settings.Default;
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Themes/{settings.Theme}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[1] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Icons/{settings.Icons}", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries[2] = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Assets/Localization/{settings.Language}", UriKind.Absolute) };

        }

        private void UCLoaded(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;
            ThemeCB.SelectedIndex = settings.Theme == "LightTheme.xaml" ? 0 : 1;
            IconsCB.SelectedIndex = settings.Icons == "MaterialDesignIconsDictionary.xaml" ? 0 : 1;
            LanguageCB.SelectedIndex = settings.Language == "ru-RU.xaml" ? 0 : 1;
            _isInitializing = false;
        }

        private void UserPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void UserPasswordBox_GotFocus(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void UserPasswordBox_LostFocus(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void CheckPlaceholder()
        {
            ChangePasswordButton.IsEnabled = UserPasswordBox.Password.Length > 0 && UserNewPasswordBox.Password.Length > 0 && RepeatUserNewPasswordBox.Password.Length > 0;
            PasswordPlaceholder.Visibility = UserPasswordBox.IsFocused || UserPasswordBox.Password.Length > 0 ?
                Visibility.Hidden : Visibility.Visible;
            NewPasswordPlaceholder.Visibility = UserNewPasswordBox.IsFocused || UserNewPasswordBox.Password.Length > 0 ?
                Visibility.Hidden : Visibility.Visible;
            RepeatNewPasswordPlaceholder.Visibility = RepeatUserNewPasswordBox.IsFocused || RepeatUserNewPasswordBox.Password.Length > 0 ?
                Visibility.Hidden : Visibility.Visible;
        }

        private async void ChangePasswordClick(object sender, RoutedEventArgs e)
        {
            using (var db = new AppDbContext())
            {
                var employee = await db.Employees.FirstOrDefaultAsync(e => e.ID == _currentUserID);
                var VerifyPassword = await Task.Run(() => BCrypt.Net.BCrypt.Verify(UserPasswordBox.Password.Trim(), employee!.PasswordHash));
                if (!VerifyPassword)
                {
                    MessageBox.Show(_isRu ? "Неверный пароль." : "Invalid password.", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (UserPasswordBox.Password.Trim() == UserNewPasswordBox.Password.Trim())
                {
                    MessageBox.Show(_isRu ? "Такой пароль уже стоит." : "New password cannot be the same as the current one.", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (VerifyPassword && UserNewPasswordBox.Password.Trim() == RepeatUserNewPasswordBox.Password.Trim())
                {
                    try
                    {
                        employee!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(UserNewPasswordBox.Password.Trim());
                        await db.SaveChangesAsync();
                        MessageBox.Show(_isRu ? "Пароль успешно изменен." : "Password changed successfully.", _isRu ? "Уведомление" : "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        UserPasswordBox.Clear();
                        UserNewPasswordBox.Clear();
                        RepeatUserNewPasswordBox.Clear();
                    }
                    catch (Exception ex)
                    {
                        if (_isRu)
                        {
                            MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show($"An error has occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                else
                {
                    MessageBox.Show(_isRu ? "Новые пароли не совпадают." : "New passwords do not match.", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
