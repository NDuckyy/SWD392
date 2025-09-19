using System.ComponentModel.DataAnnotations.Schema;

namespace SWD392_backend.Models.Request
{
    public class ReviewRequest
    {
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
    }
}
