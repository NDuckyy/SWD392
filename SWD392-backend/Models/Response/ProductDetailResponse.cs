namespace SWD392_backend.Models.Response
{
    public class ProductDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public double Price { get; set; }
        public string Description { get; set; } = null!;
        public int StockInQuantity { get; set; }
        public double RatingAverage { get; set; }
        public string Sku { get; set; } = null!;
        public double DiscountPrice { get; set; }
        public double DiscountPercent { get; set; }
        public int SoldQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsSale { get; set; }
        public string Slug { get; set; } = null!;
        public List<ProductImageResponse> Images { get; set; }
        public CategoryResponse categories { get; set; }
        public SupplierrResponse supplier { get; set; }
    }
}
