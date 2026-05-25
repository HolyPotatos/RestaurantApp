using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersViewUC.xaml
    /// </summary>
    public partial class OrdersViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
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
                        .Include(o => o.Employee!)
                        .Include(o => o.SeatTable!)
                        .Include(o => o.PaymentType!)
                        .Include(o => o.OrderDetails!)
                            .ThenInclude(od => od.Menu)
                        .OrderByDescending(o => o.ID)
                        .Where(o => o.ID.ToString().Contains(filter)
                        || o.OrderDate.ToString().Contains(filter)
                        || o.Price.ToString().Contains(filter)
                        || o.SeatTable!.TableNumber.Contains(filter)
                        || o.Employee!.LastName.Contains(filter)
                        || o.PaymentType!.Title.Contains(filter))
                        .ToListAsync();
                    OrdersGrid.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                if (_isRu)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
                }
                else
                {
                    MessageBox.Show($"Error when loading data: {ex.Message}");
                }

            }
        }

        private async void SearchTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoadDataAsync(SearchTB.Text);
            }
        }

        private async void SearchClick(object sender, RoutedEventArgs e) => await LoadDataAsync(SearchTB.Text);

        private async void AddClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        private async void EditClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        private async void DeleteClick(object sender, RoutedEventArgs e)
        {
            var answer = new MessageBoxResult();
            if (_isRu)
            {
                answer = MessageBox.Show($"Вы действительно хотите удалить запись {((CustomerOrder)OrdersGrid.SelectedItem).ID}ID без возможности восстановления?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                answer = MessageBox.Show($"Do you really want to delete the {((CustomerOrder)OrdersGrid.SelectedItem).ID}ID record permanently?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        db.CustomerOrders.Remove((CustomerOrder)OrdersGrid.SelectedItem);
                        await db.SaveChangesAsync();
                    }
                    await LoadDataAsync(SearchTB.Text);
                }
                catch (Exception ex)
                {
                    if (_isRu)
                    {
                        MessageBox.Show($"Произошла ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"An error occurred while deleting the record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
        }

        private async void OrdersLoaded(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = OrdersGrid.SelectedItem != null;
            EditButton.IsEnabled = OrdersGrid.SelectedItem != null;
        }

        private void MoreInfoClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var dataItem = btn.DataContext;
            var row = (DataGridRow)OrdersGrid.ItemContainerGenerator.ContainerFromItem(dataItem);
            if (row != null)
            {
                row.DetailsVisibility = btn.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void InnerDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
