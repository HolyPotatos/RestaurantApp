using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddEditEmployeeWindow.xaml
    /// </summary>
    public partial class AddEditEmployeeWindow : Window
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        private Employee _currentEmployee;
        public AddEditEmployeeWindow(Employee? employee = null)
        {
            InitializeComponent();
            _currentEmployee = employee;

            if (_currentEmployee != null)
            {
                WindowTitleTB.Text = _isRu ? "Редактирование сотрудника" : "Edit Employee";
            }
            else
            {
                WindowTitleTB.Text = _isRu ? "Новый сотрудник" : "New Employee";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    TitleComboBox.ItemsSource = await db.EmployeeTitles.AsNoTracking().ToListAsync();
                }

                if (_currentEmployee != null)
                {
                    LastNameTB.Text = _currentEmployee.LastName;
                    FirstNameTB.Text = _currentEmployee.FirstName;
                    MiddleNameTB.Text = _currentEmployee.MiddleName;
                    PhoneTB.Text = _currentEmployee.PhoneNumber;
                    LoginTB.Text = _currentEmployee.Login;
                    TitleComboBox.SelectedValue = _currentEmployee.EmployeeTitleID;
                }
                else
                {
                    if (TitleComboBox.Items.Count > 0)
                        TitleComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                if (_isRu)
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameTB.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTB.Text) ||
                string.IsNullOrWhiteSpace(PhoneTB.Text) ||
                string.IsNullOrWhiteSpace(LoginTB.Text) ||
                TitleComboBox.SelectedValue == null)
            {
                if (_isRu)
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Please fill in all required fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    bool loginExists = await db.Employees.AnyAsync(emp => emp.Login == LoginTB.Text && (_currentEmployee == null || emp.ID != _currentEmployee.ID));
                    if (loginExists)
                    {
                        if (_isRu)
                            MessageBox.Show("Сотрудник с таким логином уже существует!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        else
                            MessageBox.Show("An employee with this username already exists!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (_currentEmployee == null)
                    {
                        var newEmployee = new Employee
                        {
                            LastName = LastNameTB.Text,
                            FirstName = FirstNameTB.Text,
                            MiddleName = MiddleNameTB.Text,
                            PhoneNumber = PhoneTB.Text,
                            Login = LoginTB.Text,
                            isActive = true,
                            EmployeeTitleID = (int)TitleComboBox.SelectedValue,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345")
                        };
                        db.Employees.Add(newEmployee);
                    }
                    else
                    {
                        var empToUpdate = await db.Employees.FindAsync(_currentEmployee.ID);
                        if (empToUpdate != null)
                        {
                            empToUpdate.LastName = LastNameTB.Text;
                            empToUpdate.FirstName = FirstNameTB.Text;
                            empToUpdate.MiddleName = MiddleNameTB.Text;
                            empToUpdate.PhoneNumber = PhoneTB.Text;
                            empToUpdate.Login = LoginTB.Text;
                            empToUpdate.isActive = true;
                            empToUpdate.EmployeeTitleID = (int)TitleComboBox.SelectedValue;

                        }
                    }

                    await db.SaveChangesAsync();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                if (_isRu)
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void PhoneTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("8") || digits.StartsWith("7")) digits = digits.Substring(1);
            if (digits.Length > 10) digits = digits.Substring(0, 10);

            string formatted = "";
            if (digits.Length > 0)
            {
                formatted = "8(" + digits.Substring(0, Math.Min(3, digits.Length));
                if (digits.Length >= 3)
                {
                    formatted += ")" + digits.Substring(3, Math.Min(3, digits.Length - 3));
                    if (digits.Length >= 6)
                    {
                        formatted += "-" + digits.Substring(6, Math.Min(2, digits.Length - 6));
                        if (digits.Length >= 8)
                        {
                            formatted += "-" + digits.Substring(8, Math.Min(2, digits.Length - 8));
                        }
                    }
                }
            }

            if (tb.Text != formatted)
            {
                tb.Text = formatted;
                tb.CaretIndex = tb.Text.Length;
            }
        }
    }
}
