using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeeViewUC.xaml
    /// </summary>
    public partial class EmployeeViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public EmployeeViewUC()
        {
            InitializeComponent();
        }
        private async Task LoadDataAsync(string filter = "")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var employees = await db.Employees
                        .AsNoTracking()
                        .Include(e => e.EmployeeTitle)
                        .OrderByDescending(e => e.isActive)
                        .Where(e => e.ID.ToString().Contains(filter)
                        || e.FirstName.Contains(filter)
                        || e.LastName.Contains(filter)
                        || e.MiddleName!.Contains(filter)
                        || e.PhoneNumber.Contains(filter)
                        || e.Login.Contains(filter)
                        || e.EmployeeTitle!.Title.Contains(filter))
                        .ToListAsync();
                    EmployeeGrid.ItemsSource = employees;
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
        private async void BlockClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Employee)EmployeeGrid.SelectedItem;
            var answer = new MessageBoxResult();
            string actionRu = selectedItem.isActive ? "заблокировать" : "разблокировать";
            string actionEn = selectedItem.isActive ? "block" : "unblock";
            if (_isRu)
            {
                answer = MessageBox.Show($"Вы действительно хотите {actionRu} сотрудника с ID{selectedItem.ID}?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                answer = MessageBox.Show($"Are you sure you want to {actionEn} the employee with ID{selectedItem.ID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var employee = await db.Employees.FirstOrDefaultAsync(e => e.ID == selectedItem.ID);
                        employee!.isActive = !employee.isActive;
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

        private async void EmployeeLoaded(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void EmployeeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BlockButton.IsEnabled = EmployeeGrid.SelectedItem != null;
            ResetPasswordButton.IsEnabled = EmployeeGrid.SelectedItem != null;
            EditButton.IsEnabled = EmployeeGrid.SelectedItem != null;
            if (EmployeeGrid.SelectedItem != null)
            {
                BlockButton.Style = ((Employee)EmployeeGrid.SelectedItem!).isActive ?
                (Style)FindResource("EmployeeBlockButtonStyle") :
                (Style)FindResource("EmployeeUnblockButtonStyle");
            }
            else
            {
                BlockButton.Style = (Style)FindResource("EmployeeBlockButtonStyle");
            }
        }

        private async void ResetPasswordClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Employee)EmployeeGrid.SelectedItem;
            var answer = new MessageBoxResult();
            if (_isRu)
            {
                answer = MessageBox.Show($"Вы действительно хотите сбросить пароль сотруднику с ID{selectedItem.ID} до стандартного (12345)?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                answer = MessageBox.Show($"Are you sure you want to reset the password for employee ID {selectedItem.ID} to the default (12345)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var employee = await db.Employees.FirstOrDefaultAsync(e => e.ID == selectedItem.ID);
                        employee!.PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345");
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
    }
}
