using RestaurantApp.Views.Pages;
using System.Windows;

namespace RestaurantApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentUserID;
        public MainWindow(int currentUserID)
        {
            InitializeComponent();
            _currentUserID = currentUserID;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void OrdersRB_Checked(object sender, RoutedEventArgs e) => frame.Content = new OrdersViewUC();

        private void SettingRB_Checked(object sender, RoutedEventArgs e) => frame.Content = new SettingViewUC(_currentUserID);

        private void EmployeeRB_Checked(object sender, RoutedEventArgs e) => frame.Content = new EmployeeViewUC();

        private void MenuRB_Checked(object sender, RoutedEventArgs e) => frame.Content = new MenuViewUC();

        private void ReservationRB_Checked(object sender, RoutedEventArgs e) => frame.Content = new ReservationViewUC();

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var lw = new LoginWindow();
            lw.Show();
            this.Close();
        }

    }
}
