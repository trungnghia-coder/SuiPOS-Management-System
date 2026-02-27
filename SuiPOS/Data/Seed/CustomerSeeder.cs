using SuiPOS.Models;

namespace SuiPOS.Data.Seed
{
    public static class CustomerSeeder
    {
        private static readonly string[] FirstNames =
        {
            "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Phan", "Vũ", "Đặng", "Bùi", "Đỗ",
            "Hồ", "Ngô", "Dương", "Lý", "Võ", "Đinh", "Mai", "Trịnh", "Tô", "Hà"
        };

        private static readonly string[] MiddleNames =
        {
            "Văn", "Thị", "Minh", "Hữu", "Đức", "Anh", "Thu", "Hoàng", "Quốc", "Thanh",
            "Thành", "Hải", "Tuấn", "Hùng", "Nam", "Phương", "Linh", "Hương", "Mai", "Lan"
        };

        private static readonly string[] LastNames =
        {
            "An", "Bình", "Chi", "Dũng", "Giang", "Hà", "Hiếu", "Khánh", "Long", "Minh",
            "Nam", "Phong", "Quân", "Sơn", "Tâm", "Tuấn", "Uyên", "Việt", "Xuân", "Yến"
        };

        public static List<Customer> GenerateCustomers(int count = 50)
        {
            var customers = new List<Customer>();
            var random = new Random();
            var timestamp = DateTime.UtcNow.Ticks;

            for (int i = 0; i < count; i++)
            {
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var middleName = MiddleNames[random.Next(MiddleNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var fullName = $"{firstName} {middleName} {lastName}";

                // Generate unique phone with timestamp to avoid duplicates
                var phoneNumber = $"0{random.Next(3, 10)}{(timestamp + i).ToString().Substring(8, 8)}";

                var debtAmount = random.Next(0, 100) > 70 ? random.Next(50000, 500000) : 0;
                var totalSpent = random.Next(100000, 10000000);

                customers.Add(new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = fullName,
                    Phone = phoneNumber,
                    Points = random.Next(0, 1000),
                    DebtAmount = debtAmount,
                    TotalSpent = totalSpent,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365)),
                    IsActive = true
                });
            }

            return customers;
        }

    }
}
