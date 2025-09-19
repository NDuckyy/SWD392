namespace SWD392_backend.Models.Request
{
    public class AddProductRequest
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int StockInQuantity { get; set; }
        public double DiscountPercent { get; set; }
        public bool IsSale { get; set; }
        public string Sku { get; set; }
        public int CategoriesId { get; set; }
    }
}
