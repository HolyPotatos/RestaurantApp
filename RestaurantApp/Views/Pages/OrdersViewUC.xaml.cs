using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersViewUC.xaml
    /// </summary>
    public partial class OrdersViewUC : UserControl
    {
        public OrdersViewUC()
        {
            InitializeComponent();
        }
        private async Task LoadDataAsync(string filter = "")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var orders = await db.CustomerOrders
                        .AsNoTracking()
                        .Include(o => o.Employee)
                        .Include(o => o.SeatTable)
                        .Include(o => o.PaymentType)
                        .OrderByDescending(o => o.ID)
                        .Where(o => o.ID.ToString().Contains(filter) 
                        || o.OrderDate.ToString().Contains(filter) 
                        || o.PriceWithDiscount.ToString().Contains(filter)
                        || o.SeatTable.TableNumber.Contains(filter)
                        || o.Employee.LastName.Contains(filter)
                        || o.PaymentType.Title.Contains(filter))
                        .ToListAsync();
                    OrdersGrid.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                if (Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml"))
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
                }
                else
                {
                    MessageBox.Show($"Error when loading data: {ex.Message}");
                }
                
            }
        }

        private async void SearchTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                await LoadDataAsync(SearchTB.Text);
            }
        }

        private async void SearchClick(object sender, RoutedEventArgs e) => await LoadDataAsync(SearchTB.Text);

        private void AddClick(object sender, RoutedEventArgs e)
        {

        }
        private void EditClick(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Вы действительно хотите удалить запись {((CustomerOrder)OrdersGrid.SelectedItem).ID}");
        }

        private async void OrdersLoaded(object sender, RoutedEventArgs e) => await LoadDataAsync();
    }
}
