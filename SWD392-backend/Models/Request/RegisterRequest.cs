namespace cybersoft_final_project.Models.Request;

public class RegisterRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string Fullname { get; set; } = string.Empty;
}
