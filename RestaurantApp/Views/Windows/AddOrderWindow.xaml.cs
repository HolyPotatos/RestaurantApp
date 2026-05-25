using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RestaurantApp.Views.Windows
{
    public class OrderItemViewModel
    {
        public int MenuID { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для AddOrderWindow.xaml
    /// </summary>
    public partial class AddOrderWindow : Window
    {
        private CustomerOrder _order;
        private bool _isEdit;
        private AppDbContext _db;

        public ObservableCollection<OrderItemViewModel> Composition { get; set; } = new ObservableCollection<OrderItemViewModel>();

        public AddOrderWindow(CustomerOrder? order = null)
        {
            InitializeComponent();
            _db = new AppDbContext();
            _isEdit = order != null;
            OrderCompositionGrid.ItemsSource = Composition;

            if (order != null)
            {
                _order = order;
            }
            else
            {
                _order = new CustomerOrder();
                _order.OrderDate = DateTime.UtcNow.AddHours(9);
            }

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                CategoriesListBox.ItemsSource = await _db.MenuCategories.AsNoTracking().ToListAsync();
                TableCB.ItemsSource = await _db.SeatTables.AsNoTracking().ToListAsync();
                PaymentCB.ItemsSource = await _db.PaymentTypes.AsNoTracking().ToListAsync();
                MenuDataGrid.ItemsSource = await _db.Menus.AsNoTracking().ToListAsync();

                if (_isEdit)
                {
                    TableCB.SelectedValue = _order.SeatTableID;
                    PaymentCB.SelectedValue = _order.PaymentTypeID;

                    var details = await _db.OrderDetails
                        .Where(d => d.CustomerOrderID == _order.ID)
                        .Include(d => d.Menu)
                        .ToListAsync();

                    Composition.Clear();
                    foreach (var d in details)
                    {
                        if (d.Menu != null)
                        {
                            OrderItemViewModel item = new OrderItemViewModel
                            {
                                MenuID = d.MenuID,
                                MenuTitle = d.Menu.Title,
                                Price = d.Menu.Price,
                                Quantity = d.Quantity
                            };
                            Composition.Add(item);
                        }
                    }
                }
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(IsRussian() ? $"Ошибка загрузки данных: {ex.Message}" : $"Data load error: {ex.Message}");
            }
        }

        private bool IsRussian()
        {
            var currentLanguageDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Localization"));

            if (currentLanguageDict != null && currentLanguageDict.Source.OriginalString.Contains("en-US"))
            {
                return false;
            }
            return true;
        }

        private async void CategoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MenuCategory? cat = CategoriesListBox.SelectedItem as MenuCategory;
            if (cat != null)
            {
                MenuDataGrid.ItemsSource = await _db.Menus.Where(m => m.MenuCategoryID == cat.ID).AsNoTracking().ToListAsync();
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchDishTB.Text.ToLower();
            MenuDataGrid.ItemsSource = await _db.Menus.Where(m => m.Title.ToLower().Contains(query)).AsNoTracking().ToListAsync();
        }

        private void Plus_Click(object sender, RoutedEventArgs e) => ModifyQty(1);
        private void Minus_Click(object sender, RoutedEventArgs e) => ModifyQty(-1);

        private void ModifyQty(int delta)
        {
            int res = 0;
            if (int.TryParse(QuantityTB.Text, out res))
            {
                if (res + delta > 0)
                {
                    QuantityTB.Text = (res + delta).ToString();
                }
            }
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            Models.Menu? dish = MenuDataGrid.SelectedItem as Models.Menu;
            if (dish != null)
            {
                int qty = int.Parse(QuantityTB.Text);
                OrderItemViewModel? existing = Composition.FirstOrDefault(x => x.MenuID == dish.ID);

                if (existing != null)
                {
                    existing.Quantity += qty;
                }
                else
                {
                    OrderItemViewModel newItem = new OrderItemViewModel
                    {
                        MenuID = dish.ID,
                        MenuTitle = dish.Title,
                        Price = dish.Price,
                        Quantity = qty
                    };
                    Composition.Add(newItem);
                }

                OrderCompositionGrid.Items.Refresh();
                UpdateTotal();
            }
        }

        private void RemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn != null)
            {
                OrderItemViewModel? item = btn.DataContext as OrderItemViewModel;
                if (item != null)
                {
                    Composition.Remove(item);
                    UpdateTotal();
                }
            }
        }

        private void UpdateTotal()
        {
            decimal total = Composition.Sum(item => item.Price * item.Quantity);
            TotalSumText.Text = total.ToString("N2");
        }

        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (TableCB.SelectedValue == null || PaymentCB.SelectedValue == null)
            {
                MessageBox.Show(IsRussian() ? "Заполните столик и тип оплаты!" : "Please select Table and Payment Type!");
                return;
            }

            if (Composition.Count == 0)
            {
                MessageBox.Show(IsRussian() ? "Добавьте блюда в заказ!" : "Please add dishes to the order!");
                return;
            }

            var firstEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync();
            if (firstEmployee == null)
            {
                MessageBox.Show(IsRussian() ? "В базе данных нет ни одного сотрудника!" : "No employees found in the database!");
                return;
            }

            _order.SeatTableID = (int)TableCB.SelectedValue;
            _order.PaymentTypeID = (int)PaymentCB.SelectedValue;
            _order.Price = decimal.Parse(TotalSumText.Text);
            _order.EmployeeID = firstEmployee.ID;

            try
            {
                if (!_isEdit)
                {
                    _db.CustomerOrders.Add(_order);
                }
                else
                {
                    _db.CustomerOrders.Update(_order);
                }

                await _db.SaveChangesAsync();

                var oldDetails = _db.OrderDetails.Where(d => d.CustomerOrderID == _order.ID);
                _db.OrderDetails.RemoveRange(oldDetails);
                await _db.SaveChangesAsync();

                foreach (var item in Composition)
                {
                    OrderDetails od = new OrderDetails
                    {
                        CustomerOrderID = _order.ID,
                        MenuID = item.MenuID,
                        Quantity = item.Quantity
                    };
                    _db.OrderDetails.Add(od);
                }

                await _db.SaveChangesAsync();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(IsRussian() ? $"Ошибка при сохранении: {innerMsg}" : $"Saving error: {innerMsg}");
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CategoriesListBox.SelectedIndex = 0;
        }
    }
}
