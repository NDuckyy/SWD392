using System.ComponentModel.DataAnnotations;

namespace SWD392_backend.Models.Response
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }
}
