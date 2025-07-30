namespace UsersApp.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }
}
