namespace SWD392_backend.Models.Response
{
    public class ReportOrderResponse
    {
        public int year {  get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int TotalOrders { get; set; }
        public PagedResult<OrderResponse> orders { get; set; }
    }
}
