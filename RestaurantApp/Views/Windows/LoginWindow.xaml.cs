using System.Windows;

namespace RestaurantApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для окна авторизации.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void UserPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void UserPasswordBox_GotFocus(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void UserPasswordBox_LostFocus(object sender, RoutedEventArgs e) => CheckPlaceholder();
        private void CheckPlaceholder() => PasswordPlaceholder.Visibility = UserPasswordBox.IsFocused || UserPasswordBox.Password.Length > 0 ?
                                           Visibility.Hidden : Visibility.Visible;


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginErrorTB.Visibility = Visibility.Collapsed;
            PasswordErrorTB.Visibility = Visibility.Collapsed;
            WrongErrorTB.Visibility = Visibility.Collapsed;
            BlockErrorTB1.Visibility = Visibility.Collapsed;
            BlockErrorTB2.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(LoginTextBox.Text))
            {
                LoginErrorTB.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrEmpty(UserPasswordBox.Password))
            {
                PasswordErrorTB.Visibility = Visibility.Visible;
            }
            if (LoginErrorTB.Visibility == Visibility.Visible || PasswordErrorTB.Visibility == Visibility.Visible) return;

            LoginButton.IsEnabled = false;
            try
            {
                using (var db = new AppDbContext())
                {

                    var employee = await db.AuthAsync(LoginTextBox.Text.Trim(), UserPasswordBox.Password.Trim());
                    if (employee == null)
                    {
                        WrongErrorTB.Visibility = Visibility.Visible;
                        return;
                    }
                    if (!employee.isActive)
                    {
                        BlockErrorTB1.Visibility = Visibility.Visible;
                        BlockErrorTB2.Visibility = Visibility.Visible;
                        return;
                    }
                    var mw = new MainWindow(employee.ID, employee.EmployeeTitleID == 1 ? true : false);
                    mw.Show();
                    this.Close();
                }
            }
            catch
            {
                MessageBox.Show(_isRu ? "Ошибка подключения к базе данных." : "Error connecting to the database.", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
