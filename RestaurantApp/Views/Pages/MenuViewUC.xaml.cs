using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для MenuViewUC.xaml
    /// </summary>
    public partial class MenuViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public MenuViewUC()
        {
            InitializeComponent();
        }
        private async Task LoadDataAsync(string filter = "")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var menus = await db.Menus
                        .AsNoTracking()
                        .Include(m => m.MenuCategory)
                        .OrderByDescending(m => m.isActive)
                        .Where(m => m.ID.ToString().Contains(filter)
                        || m.Title.Contains(filter)
                        || m.MenuCategory!.Title.Contains(filter))
                        .ToListAsync();
                    MenuGrid.ItemsSource = menus;
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
        private async void InactiveClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Models.Menu)MenuGrid.SelectedItem;
            var answer = new MessageBoxResult();
            string actionRu = selectedItem.isActive ? "неактивным" : "активным";
            string actionEn = selectedItem.isActive ? "inactive" : "active";
            if (_isRu)
            {
                answer = MessageBox.Show($"Вы действительно хотите сделать {actionRu} блюдо {selectedItem.Title}?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                answer = MessageBox.Show($"Are you sure you want to make the dish {selectedItem.Title} {actionEn}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var menu = await db.Menus.FirstOrDefaultAsync(m => m.ID == selectedItem.ID);
                        menu!.isActive = !menu.isActive;
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

        private async void MenuLoaded(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void MenuGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InactiveButton.IsEnabled = MenuGrid.SelectedItem != null;
            EditButton.IsEnabled = MenuGrid.SelectedItem != null;
            if (MenuGrid.SelectedItem != null)
            {
                InactiveButton.Style = ((Models.Menu)MenuGrid.SelectedItem!).isActive ?
                (Style)FindResource("MenuBlockButtonStyle") :
                (Style)FindResource("MenuUnblockButtonStyle");
            }
            else
            {
                InactiveButton.Style = (Style)FindResource("MenuBlockButtonStyle");
            }
        }
    }
}
