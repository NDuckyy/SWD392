namespace SWD392_backend.Models
{
    public class PagedResult<T>
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPage => (int)Math.Ceiling((double)TotalItems / PageSize);
        public List<T> Items { get; set; }
    }
}
