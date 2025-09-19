using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SWD392_backend.Models.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
    }
}
