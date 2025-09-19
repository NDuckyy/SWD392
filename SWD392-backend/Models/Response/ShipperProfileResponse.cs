namespace SWD392_backend.Models.Response
{
    public class ShipperProfileResponse
    {
        public int Id { get; set; }
        public string AreaCode { get; set; }
        public UserProfileResponse UserProfile { get; set; }
    }
}
