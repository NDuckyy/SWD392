namespace SWD392_backend.Models.Response
{
    public class SupplierProfileResponse
    {
        public int Id { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string ImageUrl { get; set; }
        public bool IsVerified { get; set; }
        public string FrontImageCCCD { get; set; }
        public string BackImageCCCD { get; set; }
        public UserProfileResponse UserProfile { get; set; }
    }
}
