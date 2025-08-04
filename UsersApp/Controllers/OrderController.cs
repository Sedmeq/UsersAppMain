// OrderController.cs - Monthly grouping ilə
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UsersApp.Attributes;
using UsersApp.Data;
using UsersApp.Models;
using UsersApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace UsersApp.Controllers
{
    [AllRoles]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<Users> _userManager;

        public OrderController(AppDbContext context, ILogger<OrderController> logger, UserManager<Users> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderViewModels = orders.Select(o => new OrderListViewModel
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Notes = o.Notes,
                OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                }).ToList()
            }).ToList();

            // Group by month and year
            var monthlyGroups = orderViewModels
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyOrderGroupViewModel
                {
                    MonthNumber = g.Key.Month,
                    Year = g.Key.Year,
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    TotalItems = g.Sum(o => o.TotalItems),
                    Orders = g.OrderByDescending(o => o.OrderDate).ToList()
                })
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.MonthNumber)
                .ToList();

            return View(monthlyGroups);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _context.Products.ToListAsync();

            if (!products.Any())
            {
                TempData["ErrorMessage"] = "No products available. Please add products first.";
                return RedirectToAction("Index", "Product");
            }

            var model = new CreateOrderViewModel
            {
                Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - ${p.Price:F2}"
                }).ToList(),
                OrderItems = new List<OrderItemViewModel> { new OrderItemViewModel() }
            };

            // Get all users for customer selection
            await PopulateCustomersList(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            _logger.LogInformation("Create Order POST method called");

            try
            {
                // Debug: ModelState kontrol et
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid");
                    foreach (var modelError in ModelState)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            _logger.LogWarning($"Model Error - Key: {modelError.Key}, Error: {error.ErrorMessage}");
                        }
                    }
                }

                // Null check
                if (model == null)
                {
                    _logger.LogError("Model is null");
                    TempData["ErrorMessage"] = "Invalid form data received.";
                    return RedirectToAction(nameof(Create));
                }

                // OrderItems null check və temizleme
                if (model.OrderItems == null)
                {
                    model.OrderItems = new List<OrderItemViewModel>();
                }

                _logger.LogInformation($"Received {model.OrderItems.Count} order items");

                // Boş item'ları kaldır
                var validOrderItems = model.OrderItems.Where(oi => oi.ProductId > 0 && oi.Quantity > 0).ToList();

                _logger.LogInformation($"Valid order items count: {validOrderItems.Count}");

                if (validOrderItems.Count == 0)
                {
                    ModelState.AddModelError("", "Please add at least one product to the order.");
                    _logger.LogWarning("No valid order items found");
                }

                // CustomerId kontrolü ve CustomerName'i alın
                string customerName = "";
                if (string.IsNullOrWhiteSpace(model.CustomerId))
                {
                    ModelState.AddModelError(nameof(model.CustomerId), "Customer is required.");
                    _logger.LogWarning("Customer is not selected");
                }
                else
                {
                    var selectedUser = await _userManager.FindByIdAsync(model.CustomerId);
                    if (selectedUser != null)
                    {
                        customerName = selectedUser.FullName;
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(model.CustomerId), "Selected customer not found.");
                        _logger.LogWarning($"Customer not found with ID: {model.CustomerId}");
                    }
                }

                if (ModelState.IsValid && validOrderItems.Count > 0 && !string.IsNullOrEmpty(customerName))
                {
                    _logger.LogInformation("Creating new order");

                    var order = new Order
                    {
                        CustomerName = customerName,
                        Notes = model.Notes?.Trim(),
                        OrderDate = DateTime.Now,
                        OrderItems = new List<OrderItem>()
                    };

                    decimal totalAmount = 0;

                    foreach (var item in validOrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                            _logger.LogError($"Product not found: {item.ProductId}");
                            await PopulateProductsList(model);
                            await PopulateCustomersList(model);
                            return View(model);
                        }

                        var orderItem = new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            Subtotal = product.Price * item.Quantity
                        };

                        order.OrderItems.Add(orderItem);
                        totalAmount += orderItem.Subtotal;

                        _logger.LogInformation($"Added order item: Product {product.Name}, Qty: {item.Quantity}, Subtotal: {orderItem.Subtotal}");
                    }

                    order.TotalAmount = totalAmount;

                    _context.Orders.Add(order);
                    var savedCount = await _context.SaveChangesAsync();

                    _logger.LogInformation($"Order saved successfully. Saved {savedCount} records. Order ID: {order.Id}");

                    TempData["SuccessMessage"] = $"Order created successfully! Order ID: {order.Id}";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("ModelState validation failed or no valid items");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", $"An error occurred while creating the order: {ex.Message}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            // Hata durumunda products ve customers listesini yeniden doldur
            await PopulateProductsList(model);
            await PopulateCustomersList(model);

            // Eğer OrderItems boşsa, en az bir tane ekle
            if (model.OrderItems == null || !model.OrderItems.Any())
            {
                model.OrderItems = new List<OrderItemViewModel> { new OrderItemViewModel() };
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    return Json(new { price = product.Price, name = product.Name });
                }
                return Json(new { price = 0, name = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product price for ID: {productId}");
                return Json(new { price = 0, name = "" });
            }
        }

        private async Task PopulateProductsList(CreateOrderViewModel model)
        {
            var products = await _context.Products.ToListAsync();
            model.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} - ${p.Price:F2}"
            }).ToList();
        }

        private async Task PopulateCustomersList(CreateOrderViewModel model)
        {
            var users = await _userManager.Users.ToListAsync();
            model.Customers = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FullName} ({u.Email})"
            }).ToList();
        }

        // Diğer metodlar aynı kalacak...
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order != null)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order {id}");
                TempData["ErrorMessage"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}