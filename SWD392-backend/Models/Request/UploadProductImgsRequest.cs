namespace SWD392_backend.Models.Request
{
    public class UploadProductImgsRequest
    {
        public int ProductId { get; set; }
        public string ProductSlug { get; set; }
        public int SupplierId { get; set; }
        public int CategoryId {  get; set; }
        public List<string> ContentTypes { get; set; } = new();
    }
}
