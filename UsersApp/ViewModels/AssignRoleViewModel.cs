using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Select Role")]
        public string SelectedRole { get; set; }

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
