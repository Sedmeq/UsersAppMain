using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class OrderItemViewModel
    {
        [Required(ErrorMessage = "Product is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? ProductName { get; set; }
    }
}
