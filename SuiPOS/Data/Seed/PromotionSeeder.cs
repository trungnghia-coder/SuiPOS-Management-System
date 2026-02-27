using SuiPOS.Models;

namespace SuiPOS.Data.Seed
{
    public static class PromotionSeeder
    {
        private static readonly string[] PromotionNames =
        {
            "Khuyến Mãi Mùa Hè", "Sale Cuối Tuần", "Giảm Giá Đặc Biệt",
            "Flash Sale", "Khuyến Mãi Sinh Nhật", "Black Friday",
            "Cyber Monday", "Tết Sale", "Giảm Giá Sốc", "Happy Hour"
        };

        public static List<Promotion> GeneratePromotions(int count = 10)
        {
            var promotions = new List<Promotion>();
            var random = new Random();
            var startDate = new DateTime(2025, 7, 9);
            var now = DateTime.UtcNow;

            for (int i = 0; i < count; i++)
            {
                var isPercentage = random.Next(0, 2) == 0;
                var discountValue = isPercentage
                    ? random.Next(5, 50)
                    : random.Next(10, 100) * 1000;

                var minOrder = random.Next(0, 100) > 30
                    ? random.Next(100, 500) * 1000
                    : 0;

                var maxDiscount = isPercentage && discountValue > 20
                    ? random.Next(50, 200) * 1000
                    : (decimal?)null;

                var promoStart = startDate.AddDays(random.Next(0, (now - startDate).Days));
                var promoEnd = promoStart.AddDays(random.Next(7, 60));

                var isActive = promoEnd > now && promoStart <= now;

                promotions.Add(new Promotion
                {
                    Id = Guid.NewGuid(),
                    Name = $"{PromotionNames[i % PromotionNames.Length]} #{i + 1}",
                    Code = $"PROMO{DateTime.UtcNow.Ticks.ToString().Substring(8)}{i:D2}",
                    Type = isPercentage ? Promotion.DiscountType.Percentage : Promotion.DiscountType.FixedAmount,
                    DiscountValue = discountValue,
                    MinOrderAmount = minOrder,
                    MaxDiscountAmount = maxDiscount,
                    StartDate = promoStart,
                    EndDate = promoEnd,
                    IsActive = isActive
                });
            }

            return promotions;
        }
    }
}
