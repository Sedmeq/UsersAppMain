using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return View(userViewModels);
        }

        // Create User GET
        public async Task<IActionResult> CreateUser()
        {
            var allRoles = await roleManager.Roles.ToListAsync();
            var model = new CreateUserViewModel
            {
                AvailableRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // Create User POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    var allRoles = await roleManager.Roles.ToListAsync();
                    model.AvailableRoles = allRoles.Select(r => r.Name).ToList();
                    return View(model);
                }

                var user = new Users
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role if selected
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    TempData["Success"] = $"User '{model.FullName}' created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            var roles = await roleManager.Roles.ToListAsync();
            model.AvailableRoles = roles.Select(r => r.Name).ToList();
            return View(model);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var allRoles = await roleManager.Roles.ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                SelectedRole = userRoles.FirstOrDefault(),
                AvailableRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var updateResult = await userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    var currentRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var allRoles = await roleManager.Roles.ToListAsync();
            model.AvailableRoles = allRoles.Select(r => r.Name).ToList();
            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow admin to delete themselves
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser.Id == user.Id)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("Index");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = userRoles.FirstOrDefault() ?? "No Role"
            };

            return View(model);
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser.Id == user.Id)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("Index");
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Error deleting user!";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AssignRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var allRoles = await roleManager.Roles.ToListAsync();

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserEmail = user.Email,
                SelectedRole = userRoles.FirstOrDefault(),
                AvailableRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    var result = await userManager.AddToRoleAsync(user, model.SelectedRole);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Role assigned successfully!";
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            var allRoles = await roleManager.Roles.ToListAsync();
            model.AvailableRoles = allRoles.Select(r => r.Name).ToList();
            return View(model);
        }
    }
}