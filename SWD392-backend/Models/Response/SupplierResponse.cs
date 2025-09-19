    namespace SWD392_backend.Models.Response;

    public class SupplierResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public bool IsVerified { get; set; }
        public string? FrontImageCCCD { get; set; }
        public string? BackImageCCCD { get; set; }
        public DateTime RegisteredAt { get; set; }

        // Thêm các thông tin từ bảng User
        public int UserId { get; set; }  // Thêm UserId
    }