using System.Text.Json.Serialization;

namespace SWD392_backend.Models.Response
{
    public class ProductResponse
    {
        // Basic info
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;

        // Pricing
        public double Price { get; set; }
        public double DiscountPrice { get; set; }
        public double DiscountPercent { get; set; }

        // Rating & Sale
        public double RatingAverage { get; set; }
        public bool IsSale { get; set; }

        // Inventory
        public int StockInQuantity { get; set; }
        public int SoldQuantity { get; set; }

        // Relations
        public int CategoriesId { get; set; }
        public string CategoryName { get; set; }
        public int SupplierId { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
