using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
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
    /// Логика взаимодействия для ReservationViewUC.xaml
    /// </summary>
    public partial class ReservationViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public ReservationViewUC()
        {
            InitializeComponent();
        }
        private async Task LoadDataAsync(string filter = "")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var reservations = await db.Reservations
                        .AsNoTracking()
                        .Include(r => r.SeatTable!)
                        .Include(r => r.ReservationStatus!)
                        .OrderByDescending(r => r.ReservationDateStart)
                        .Where(o => o.ID.ToString().Contains(filter)
                        || o.CustomerName.Contains(filter)
                        || o.CustomerPhone.Contains(filter)
                        || o.SeatTable!.TableNumber.Contains(filter))
                        .ToListAsync();
                    ReservationGrid.ItemsSource = reservations;
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
        private async void CancelClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Reservation)ReservationGrid.SelectedItem;
            var answer = new MessageBoxResult();
            if (_isRu)
            {
                answer = MessageBox.Show($"Вы действительно хотите отменить бронирование с ID {selectedItem.ID}?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                answer = MessageBox.Show($"Are you sure you want to cancel reservation ID {selectedItem.ID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.ID == selectedItem.ID);
                        reservation!.ReservationStatusID = 3;
                        await db.SaveChangesAsync();
                    }
                    await LoadDataAsync(SearchTB.Text);
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
        }

        private async void ReservationLoaded(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void ReservationGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            CancelButton.IsEnabled = ReservationGrid.SelectedItem != null && ((Reservation)ReservationGrid.SelectedItem).ReservationStatus!.Title == "Активен";
        
    }
}
