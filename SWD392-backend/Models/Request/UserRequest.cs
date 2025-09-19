namespace SWD392_backend.Models.Requests
{
    public class UserRequest
    {
        public string? Username { get; set; }
        
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}