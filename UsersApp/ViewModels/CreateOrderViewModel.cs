using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
    }
}
