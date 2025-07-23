using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Name is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [StringLength(40, MinimumLength =8, ErrorMessage ="The {0} must be at {2} and at max {1} charachter")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword" , ErrorMessage ="Password does not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
