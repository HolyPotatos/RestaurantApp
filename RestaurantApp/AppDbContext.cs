using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System.Configuration;

namespace RestaurantApp
{ 
    public class AppDbContext : DbContext
    {
        
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeTitle> EmployeeTitles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationStatus> ReservationStatuses { get; set; }
        public DbSet<SeatTable> SeatTables { get; set; }
        public DbSet<TableStatus> TableStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        public async Task<Employee?> AuthAsync(string login, string password)
        {
            var employee = await Employees.FirstOrDefaultAsync(x => x.Login == login);
            if (employee == null) return null;
            var VerifyPassword = false;
            try
            {
                VerifyPassword = await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash));
            }
            catch
            {
                VerifyPassword = false;
            }
            if (VerifyPassword) { return employee; }
            return null;
        }
    }
}
