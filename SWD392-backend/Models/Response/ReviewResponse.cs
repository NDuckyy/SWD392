using System.ComponentModel.DataAnnotations.Schema;

namespace SWD392_backend.Models.Response
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserResponse user { get; set; } = null!;
    }
}
