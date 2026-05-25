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
using System.Windows.Shapes;

namespace RestaurantApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddReservationWindow.xaml
    /// </summary>
    public partial class AddReservationWindow : Window
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public AddReservationWindow()
        {
            InitializeComponent();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void PhoneTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("8") || digits.StartsWith("7"))
                digits = digits.Substring(1);
            if (digits.Length > 10)
                digits = digits.Substring(0, 10);
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

        private void TimeTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());

            string formatted = digits;
            if (digits.Length == 2)
            {

            }
            if (digits.Length >= 3)
            {
                formatted = digits.Substring(0, 2) + ":" + digits.Substring(2);
            }
            if (tb.Text != formatted)
            {
                tb.Text = formatted;
                tb.CaretIndex = tb.Text.Length;
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
                    var tables = await db.SeatTables
                        .AsNoTracking()
                        .OrderBy(t => t.TableNumber)
                        .ToListAsync();

                    TableComboBox.ItemsSource = tables;
                    if (tables.Any())
                        TableComboBox.SelectedIndex = 0; 
                }
            }
            catch (Exception ex)
            {
                if(_isRu)
                    MessageBox.Show($"Ошибка при загрузке столиков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Error loading tables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GuestNameTB.Text) ||
                string.IsNullOrWhiteSpace(PhoneTB.Text) ||
                string.IsNullOrWhiteSpace(TimeStartTB.Text) ||
                string.IsNullOrWhiteSpace(TimeEndTB.Text) ||
                TableComboBox.SelectedValue == null)
            {
                if(_isRu)
                    MessageBox.Show("Пожалуйста, заполните все поля формы.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Please fill in all form fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(TimeStartTB.Text, out TimeSpan startTime) ||
                !TimeSpan.TryParse(TimeEndTB.Text, out TimeSpan endTime))
            {
                if (_isRu)
                    MessageBox.Show("Неверный формат времени. Используйте ЧЧ:ММ (например, 18:00).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Invalid time format. Please use HH:mm (e.g., 18:00).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DateTime baseDate = ReservationDatePicker.SelectedDate ?? DateTime.Today;
                DateTimeOffset dateStart = new DateTimeOffset(baseDate.Year, baseDate.Month, baseDate.Day, startTime.Hours, startTime.Minutes, 0, TimeSpan.Zero);
                DateTimeOffset dateEnd = new DateTimeOffset(baseDate.Year, baseDate.Month, baseDate.Day, endTime.Hours, endTime.Minutes, 0, TimeSpan.Zero);

                int selectedTableId = (int)TableComboBox.SelectedValue;

                using (var db = new AppDbContext())
                {
                    bool isConflict = await db.Reservations.AnyAsync(r =>
                        r.SeatTableID == selectedTableId &&
                        r.ReservationStatusID == 1 &&
                        r.ReservationDateStart < dateEnd &&
                        r.ReservationDateEnd > dateStart);

                    if (isConflict)
                    {
                        if(_isRu)
                            MessageBox.Show("Этот столик уже забронирован на выбранное время!", "Конфликт бронирования", MessageBoxButton.OK, MessageBoxImage.Warning);
                        else
                            MessageBox.Show("This table is already booked for the selected time!", "Booking Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var newReservation = new Reservation
                    {
                        CustomerName = GuestNameTB.Text,
                        CustomerPhone = PhoneTB.Text,
                        ReservationDateStart = dateStart,
                        ReservationDateEnd = dateEnd,
                        SeatTableID = selectedTableId,
                        ReservationStatusID = 1
                    };
                    db.Reservations.Add(newReservation);
                    await db.SaveChangesAsync();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                if(_isRu)
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
