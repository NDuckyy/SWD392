namespace SWD392_backend.Models.Response
{
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }        
        public DateTime CreatedAt { get; set; }

    }
}
