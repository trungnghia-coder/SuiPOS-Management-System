using SuiPOS.Models;

namespace SuiPOS.Data.Seed
{
    public static class ProductSeeder
    {
        private static readonly string[] ProductPrefixes =
        {
            "Áo Thun", "Áo Sơ Mi", "Quần Jean", "Quần Kaki", "Đầm", "Váy",
            "Áo Khoác", "Giày Sneaker", "Giày Cao Gót", "Túi Xách", "Balo",
            "Nón", "Kính", "Đồng Hồ", "Vòng Tay", "Dây Chuyền", "Laptop",
            "Điện Thoại", "Tai Nghe", "Chuột"
        };

        private static readonly string[] Adjectives =
        {
            "Cao Cấp", "Sang Trọng", "Thời Trang", "Trẻ Trung", "Thanh Lịch",
            "Năng Động", "Hiện Đại", "Cổ Điển", "Đẹp", "Hot"
        };

        private static readonly Guid[] CategoryIds =
        {
            Guid.Parse("c51991ee-3308-451d-bc9e-0be8526b31d2"), // Áo Thời Trang
            Guid.Parse("a818812f-c3df-40c3-857a-25760972cd5e"), // Giày Dép
            Guid.Parse("3d48c2b9-975f-4f82-8a26-3e7f2e174f28"), // Phụ Kiện
            Guid.Parse("031a3ba7-68b4-495e-8c01-975defde3fbb")  // Công nghệ
        };

        private static readonly Guid SizeAttributeId = Guid.Parse("d85ae2f2-b190-4d46-82d4-512ae52d91d1");
        private static readonly Guid ColorAttributeId = Guid.Parse("5881d02b-1c62-432a-b34a-7ae4f68d502f");

        private static readonly Dictionary<string, Guid> SizeValues = new()
        {
            { "S", Guid.Parse("8a77d9cd-9371-48af-932e-bc6f4caf9fa9") },
            { "M", Guid.Parse("f09c2fee-fdcb-491b-be39-eb886af1e8e2") },
            { "L", Guid.Parse("189e8cfc-e078-4746-a1d6-84889655572a") },
            { "XL", Guid.Parse("5c143728-1ef6-413c-ad64-eb9603165719") },
            { "2XL", Guid.Parse("67874405-f1f4-4a7b-9f9c-4e04a5b0c1d2") }
        };

        private static readonly Dictionary<string, Guid> ColorValues = new()
        {
            { "Đỏ", Guid.Parse("8c8b3800-78aa-4c5d-a7ba-3c323edce0c6") },
            { "Đen", Guid.Parse("a5f92e1c-4d3a-4b8e-9f7c-2e5d6a7b8c9d") },
            { "Xanh Dương", Guid.Parse("b6e03f2d-5e4b-5c9f-0a8d-3f6e7b8c9e0f") }
        };

        public static (List<Product>, List<ProductVariant>) GenerateProducts(int count = 100)
        {
            var products = new List<Product>();
            var allVariants = new List<ProductVariant>();
            var random = new Random();
            var timestamp = DateTime.UtcNow.Ticks.ToString().Substring(8);

            for (int i = 0; i < count; i++)
            {
                var productId = Guid.NewGuid();
                var prefix = ProductPrefixes[random.Next(ProductPrefixes.Length)];
                var adjective = Adjectives[random.Next(Adjectives.Length)];
                var productName = $"{prefix} {adjective} #{timestamp}-{i + 1:D3}";

                var basePrice = random.Next(100, 2000) * 1000;

                var product = new Product
                {
                    Id = productId,
                    Name = productName,
                    CategoryId = CategoryIds[random.Next(CategoryIds.Length)],
                    ImageUrl = $"https://res.cloudinary.com/dhdh6g0yg/image/upload/v1770803732/SuiPOS/products/5205578e-b488-4d42-ac9b-8dc07c32e9df.jpg",
                    isActive = true
                };

                products.Add(product);

                // Generate variants (2-4 sizes x 1-2 colors)
                var numSizes = random.Next(2, 5);
                var numColors = random.Next(1, 3);

                var selectedSizes = SizeValues.OrderBy(x => random.Next()).Take(numSizes).ToList();
                var selectedColors = ColorValues.OrderBy(x => random.Next()).Take(numColors).ToList();

                int variantIndex = 1;
                foreach (var size in selectedSizes)
                {
                    foreach (var color in selectedColors)
                    {
                        var variantPrice = basePrice + random.Next(-50000, 50000);
                        var stock = random.Next(0, 100);

                        var variant = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            SKU = $"SKU-{timestamp}-{i + 1:D3}-{variantIndex++:D2}",
                            Price = variantPrice,
                            Stock = stock,
                            VariantCombination = $"{size.Key} / {color.Key}"
                        };

                        allVariants.Add(variant);
                    }
                }
            }

            return (products, allVariants);
        }

    }
}
