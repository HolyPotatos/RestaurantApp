using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddEditMenuWindow.xaml
    /// </summary>
    public partial class AddEditMenuWindow : Window
    {
        private AppDbContext _db = new AppDbContext();
        private Models.Menu _currentMenu;
        private bool _isEditMode = false;
        private bool _isRu = false;

        public AddEditMenuWindow(Models.Menu? selectedMenu = null)
        {
            InitializeComponent();
            DetermineLanguage();

            if (selectedMenu != null)
            {
                _currentMenu = selectedMenu;
                _isEditMode = true;
            }
            else
            {
                _currentMenu = new Models.Menu { isActive = true };
                _isEditMode = false;
            }
        }

        private void DetermineLanguage()
        {
            _isRu = false;
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.Source != null && dict.Source.ToString().Contains("ru-RU.xaml"))
                {
                    _isRu = true;
                    break;
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowTitleBlock.Text = _isEditMode
                    ? (string)FindResource("MenuEditWindowEditTitle")
                    : (string)FindResource("MenuEditWindowAddTitle");
            }
            catch
            {
                WindowTitleBlock.Text = _isEditMode ? "Редактирование блюда" : "Добавление нового блюда";
            }

            try
            {
                var categories = await _db.MenuCategories.ToListAsync();
                CategoryCB.ItemsSource = categories;

                if (_isEditMode)
                {
                    var trackedMenu = await _db.Menus.FirstOrDefaultAsync(m => m.ID == _currentMenu.ID);
                    if (trackedMenu != null)
                    {
                        _currentMenu = trackedMenu;
                    }

                    TitleTB.Text = _currentMenu.Title;
                    PriceTB.Text = _currentMenu.Price.ToString("F2");
                    CategoryCB.SelectedValue = _currentMenu.MenuCategoryID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_isRu ? $"Ошибка загрузки данных: {ex.Message}" : $"Error loading data: {ex.Message}");
            }
        }

        private async void SaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTB.Text) ||
                CategoryCB.SelectedValue == null ||
                !decimal.TryParse(PriceTB.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) ||
                price < 0)
            {
                try
                {
                    MessageBox.Show((string)FindResource("MenuEditValidationError"), _isRu ? "Предупреждение" : "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch
                {
                    MessageBox.Show(_isRu ? "Заполните все поля корректно!" : "Please fill fields correctly!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            try
            {
                _currentMenu.Title = TitleTB.Text.Trim();
                _currentMenu.Price = price;
                _currentMenu.MenuCategoryID = (int)CategoryCB.SelectedValue;

                if (_isEditMode)
                {
                    _db.Entry(_currentMenu).State = EntityState.Modified;
                }
                else
                {
                    _db.Menus.Add(_currentMenu);
                }

                await _db.SaveChangesAsync();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_isRu ? $"Ошибка при сохранении: {ex.Message}" : $"Saving error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string currentText = textBox.Text;
            if (e.Text == "." || e.Text == ",")
            {
                if (currentText.Length == 0 || currentText.Contains(".") || currentText.Contains(","))
                {
                    e.Handled = true;
                }
            }
            else if (!Regex.IsMatch(e.Text, "[0-9]"))
            {
                e.Handled = true;
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _db.Dispose();
        }
    }
}
