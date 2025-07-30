using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UsersApp.Attributes;
using UsersApp.Data;
using UsersApp.Models;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    [AllRoles]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
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

            return View(orderViewModels);
        }

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

        public async Task<IActionResult> Create()
        {
            var products = await _context.Products.ToListAsync();
            
            var model = new CreateOrderViewModel
            {
                Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - ${p.Price}"
                }).ToList()
            };

            // Add one empty order item by default
            model.OrderItems.Add(new OrderItemViewModel());

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            // Remove empty order items
            model.OrderItems = model.OrderItems.Where(oi => oi.ProductId > 0 && oi.Quantity > 0).ToList();

            if (model.OrderItems.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one product to the order.");
            }

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    CustomerName = model.CustomerName,
                    Notes = model.Notes,
                    OrderDate = DateTime.Now
                };

                decimal totalAmount = 0;

                foreach (var item in model.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                        await PopulateProductsList(model);
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
                }

                order.TotalAmount = totalAmount;

                _context.Add(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateProductsList(model);
            return View(model);
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
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                return Json(new { price = product.Price, name = product.Name });
            }
            return Json(new { price = 0, name = "" });
        }

        private async Task PopulateProductsList(CreateOrderViewModel model)
        {
            var products = await _context.Products.ToListAsync();
            model.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} - ${p.Price}"
            }).ToList();
        }
    }
}