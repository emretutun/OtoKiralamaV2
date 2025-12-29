using EmreGaleriApp.Repository;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class StockItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICashRegisterService _cashRegisterService;

        public StockItemController(AppDbContext context, ICashRegisterService cashRegisterService)
        {
            _context = context;
            _cashRegisterService = cashRegisterService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var stockItems = await _context.StockItems
                .Include(s => s.Firm)
                .ToListAsync();

            return View(stockItems);
        }

        // Detay
        public async Task<IActionResult> Details(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Firm)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockItem == null) return NotFound();

            return View(stockItem);
        }

        // Create GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name");
            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockItem stockItem)
        {
            // 1) ModelState hatalarını DIREKT göster (debug)
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                return BadRequest(new
                {
                    Message = "ModelState geçersiz. Zorunlu alanlar veya format hatası var.",
                    Errors = errors
                });
            }

            // 2) Kaydetme ve kasa işlemini tek yerde yakala (debug)
            try
            {
                // Stock kaydı
                _context.StockItems.Add(stockItem);
                await _context.SaveChangesAsync();

                // Kasa hareketi
                var transaction = new CashRegister
                {
                    Amount = -(stockItem.PurchasePrice * stockItem.Quantity),
                    Type = "Gider",
                    Description = $"Stok alımı - Ürün: {stockItem.ProductName}, Adet: {stockItem.Quantity}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    RelatedEntityType = "Stock",
                    RelatedEntityId = stockItem.Id
                };

                await _cashRegisterService.AddTransactionAsync(transaction);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Gerçek hatayı NET gör
                return BadRequest(new
                {
                    Message = "Create sırasında exception oluştu.",
                    Exception = ex.Message,
                    Inner = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        // Edit GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null) return NotFound();

            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name", stockItem.FirmId);
            return View(stockItem);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockItem stockItem)
        {
            if (id != stockItem.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name", stockItem.FirmId);
                return View(stockItem);
            }

            try
            {
                _context.Update(stockItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Edit sırasında exception oluştu.",
                    Exception = ex.Message,
                    Inner = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        // Delete GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Firm)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockItem == null) return NotFound();

            return View(stockItem);
        }

        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var stockItem = await _context.StockItems.FindAsync(id);
                if (stockItem != null)
                {
                    _context.StockItems.Remove(stockItem);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Delete sırasında exception oluştu.",
                    Exception = ex.Message,
                    Inner = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}
