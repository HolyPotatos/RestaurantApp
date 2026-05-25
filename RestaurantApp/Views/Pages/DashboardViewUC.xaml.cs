using ClosedXML.Excel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RestaurantApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для DashboardViewUC.xaml
    /// </summary>
    public partial class DashboardViewUC : UserControl
    {
        private bool _isRu = Application.Current.Resources.MergedDictionaries[2].Source.ToString().Contains("ru-RU.xaml");
        public SeriesCollection ChartSeries { get; set; }
        public List<string> ChartLabels { get; set; }
        public Func<double, string> PriceFormatter { get; set; }

        private bool _isLoaded = false;

        public DashboardViewUC()
        {
            InitializeComponent();
            ChartSeries = new SeriesCollection();
            ChartLabels = new List<string>();
            PriceFormatter = value => value.ToString("N0") + " ₽";
            DataContext = this;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            await LoadDataAsync(7);
        }

        private async void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            if (sender is RadioButton rb && rb.Tag != null)
            {
                if (int.TryParse(rb.Tag.ToString(), out int days))
                {
                    await LoadDataAsync(days);
                }
            }
        }

        private async Task LoadDataAsync(int days)
        {
            try
            {
                DateTimeOffset startDate = DateTime.Today.AddDays(-days).ToUniversalTime();
                if (days > 30)
                {
                    startDate = new DateTime(startDate.Year, startDate.Month, 1).ToUniversalTime();
                }

                using (var db = new AppDbContext())
                {
                    var orders = await db.CustomerOrders
                        .AsNoTracking()
                        .Where(o => o.OrderDate >= startDate)
                        .ToListAsync();
                    decimal totalRevenue = orders.Sum(o => o.Price);
                    int ordersCount = orders.Count;
                    decimal avgCheck = ordersCount > 0 ? totalRevenue / ordersCount : 0;
                    RevenueTB.Text = totalRevenue.ToString("N2") + " ₽";
                    OrdersCountTB.Text = ordersCount.ToString();
                    AvgCheckTB.Text = avgCheck.ToString("N2") + " ₽";
                    UpdateChart(orders, days);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_isRu ? $"Ошибка загрузки графика: {ex.Message}" : $"Error dashboard load: {ex.Message}", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateChart(List<CustomerOrder> orders, int days)
        {
            ChartSeries.Clear();
            ChartLabels.Clear();

            var values = new ChartValues<double>();

            if (days <= 30)
            {
                var groupedOrders = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var group in groupedOrders)
                {
                    values.Add((double)group.Sum(o => o.Price));
                    ChartLabels.Add(group.Key.ToString("dd MMM"));
                }
            }
            else
            {
                var groupedOrders = orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .ToList();

                foreach (var group in groupedOrders)
                {
                    values.Add((double)group.Sum(o => o.Price));
                    var monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMM yy");
                    ChartLabels.Add(monthName);
                }
            }
            ChartSeries.Add(new LineSeries
            {
                Title = _isRu ? "Выручка" : "Revenue",
                Values = values,
                StrokeThickness = 3,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B4F3B")),
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#336B4F3B")),
                PointGeometrySize = 10
            });
        }

        private async void ExportClick(object sender, RoutedEventArgs e)
        {
            int days = 7;
            if (Rb1Month.IsChecked == true) days = 30;
            else if (Rb3Months.IsChecked == true) days = 90;
            else if (Rb6Months.IsChecked == true) days = 180;
            else if (Rb1Year.IsChecked == true) days = 365;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = _isRu ? "Excel Файлы (*.xlsx)|*.xlsx" : "Excel files (*.xlsx)|*.xlsx",
                FileName = _isRu ? $"Отчет_Выручка_{DateTime.Now:dd_MM_yyyy}.xlsx" : $"Report_Revenue_{DateTime.Now:yyyy_MM_dd}.xlsx",
                Title = _isRu ? "Сохранить отчет" : "Save report"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    DateTimeOffset startDate = DateTime.Today.AddDays(-days).ToUniversalTime();
                    if (days > 30)
                    {
                        startDate = new DateTime(startDate.Year, startDate.Month, 1).ToUniversalTime();
                    }

                    List<CustomerOrder> orders;
                    using (var db = new AppDbContext())
                    {
                        orders = await db.CustomerOrders
                            .AsNoTracking()
                            .Include(o => o.Employee)
                            .Include(o => o.SeatTable)
                            .Where(o => o.OrderDate >= startDate)
                            .OrderBy(o => o.OrderDate)
                            .ToListAsync();
                    }

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add(_isRu ? "Выручка" : "Revenue");

                        worksheet.Cell(1, 1).Value = _isRu ? "№ Заказа" : "Order No.";
                        worksheet.Cell(1, 2).Value = _isRu ? "Дата и время" : "Date & Time";
                        worksheet.Cell(1, 3).Value = _isRu ? "Сотрудник" : "Employee";
                        worksheet.Cell(1, 4).Value = _isRu ? "Стол" : "Table";
                        worksheet.Cell(1, 5).Value = _isRu ? "Сумма (₽)" : "Sum";
                        worksheet.Range("A1:E1").Style.Font.Bold = true;

                        int row = 2;
                        foreach (var order in orders)
                        {
                            worksheet.Cell(row, 1).Value = order.ID;
                            worksheet.Cell(row, 2).Value = order.OrderDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                            string employeeName = $"{order.Employee!.LastName} {order.Employee.FirstName}";
                            worksheet.Cell(row, 3).Value = employeeName;
                            worksheet.Cell(row, 4).Value = order.SeatTable!.TableNumber;
                            worksheet.Cell(row, 5).Value = order.Price;
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₽";
                            row++;
                        }

                        worksheet.Cell(row, 4).Value = _isRu ? "ИТОГО:" : "TOTAL:";
                        worksheet.Cell(row, 4).Style.Font.Bold = true;
                        worksheet.Cell(row, 5).FormulaA1 = $"SUM(E2:E{row - 1})";
                        worksheet.Cell(row, 5).Style.Font.Bold = true;
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₽";
                        worksheet.Range($"A1:E{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Range($"A1:E{row}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show(_isRu ? "Отчет успешно выгружен!" : "Report saved successfully!", _isRu ? "Успех" : "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_isRu ? $"Ошибка при выгрузке: {ex.Message}" : $"Export error: {ex.Message}", _isRu ? "Ошибка" : "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
