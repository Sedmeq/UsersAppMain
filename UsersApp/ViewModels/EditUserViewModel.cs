using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Role")]
        public string SelectedRole { get; set; }

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
