using WebApplication1.StructuralPatterns.Composite;


namespace Design_Patterns.StructuralPatterns.Composite
{
    // DTO nhận dữ liệu từ Postman
    public class CartItemDto
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CartBundleDto
    {
        public string Name { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<CartItemDto> Items { get; set; }
    }

    public static class CompositeExample
    {
        // Run() nhận input động thay vì fix cứng
        public static object Run(List<CartBundleDto> bundles)
        {
            var cart = new CartBundle("Shopping Cart");
            var bundlePrices = new List<object>();

            foreach (var bundleDto in bundles)
            {
                var bundle = new CartBundle(bundleDto.Name, bundleDto.DiscountAmount);

                foreach (var itemDto in bundleDto.Items)
                {
                    var item = new CartItem(itemDto.Sku, itemDto.Quantity, itemDto.UnitPrice);
                    bundle.Add(item);
                }

                cart.Add(bundle);

                bundlePrices.Add(new
                {
                    BundleName = bundleDto.Name,
                    Price = bundle.GetPrice()
                });
            }

            return new
            {
                Bundles = bundlePrices,
                TotalCartPrice = cart.GetPrice()
            };
        }
    }
}