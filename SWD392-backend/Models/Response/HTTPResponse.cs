namespace cybersoft_final_project.Models;

public class HTTPResponse<T>
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime DateTime { get; set; }

    // Thêm phương thức tĩnh để tạo HTTPResponse
    public static HTTPResponse<T> Response(int statusCode, string? message, T? data)
    {
        return new HTTPResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            Data = data,
            DateTime = DateTime.Now
        };
    }
}