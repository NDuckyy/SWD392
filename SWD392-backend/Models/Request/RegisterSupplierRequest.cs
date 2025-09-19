namespace SWD392_backend.Models.Request;

public class RegisterSupplierRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string Fullname { get; set; } = string.Empty;
    
    public string back_image { get; set; } = string.Empty;
    
    public string front_image { get; set; } = string.Empty;
}