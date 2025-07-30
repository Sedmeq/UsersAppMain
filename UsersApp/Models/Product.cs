using System.ComponentModel.DataAnnotations;

namespace UsersApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [Display(Name = "Product Price")]
        public decimal Price { get; set; }
    }
}
