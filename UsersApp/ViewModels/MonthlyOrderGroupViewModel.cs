namespace UsersApp.ViewModels
{
    public class MonthlyOrderGroupViewModel
    {
        public string MonthYear { get; set; } // "January 2025" kimi format
        public int MonthNumber { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public List<OrderListViewModel> Orders { get; set; } = new List<OrderListViewModel>();
    }
}
