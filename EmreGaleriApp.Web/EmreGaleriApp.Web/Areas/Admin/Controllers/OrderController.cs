using EmreGaleriApp.Core.Enums;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICashRegisterService _cashRegisterService;

        public OrderController(
            AppDbContext context,
            IEmailService emailService,
            IInvoiceService invoiceService,
            ICashRegisterService cashRegisterService)
        {
            _context = context;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _cashRegisterService = cashRegisterService;
        }

        // 🔸 Siparişleri listele
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .OrderByDescending(o => o.StartDate) // DateOnly ile sorunsuz
                .ToListAsync();

            return View(orders);
        }

        // 🔸 Siparişi onayla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = "Onaylandı";

            foreach (var item in order.OrderItems)
                item.Car.IsAvailable = false;

            await _context.SaveChangesAsync();

            // ✅ Kasa hareketi → UTC
            var transaction = new CashRegister
            {
                Amount = order.TotalPrice,
                Type = "Gelir",
                Description = $"Araç kiralama - Sipariş No: {order.Id}",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            };

            await _cashRegisterService.AddTransactionAsync(transaction);

            // Fatura ve mail
            var invoicePdf = _invoiceService.GenerateInvoicePdf(order);
            await _emailService.SendOrderApprovedEmail(
                order.AppUser.Email!,
                order.AppUser.UserName!,
                invoicePdf
            );

            TempData["SuccessMessage"] =
                "Sipariş onaylandı, kasa hareketi kaydedildi ve kullanıcıya mail gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        // 🔸 Siparişi reddet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = "Reddedildi";
            await _context.SaveChangesAsync();

            await _emailService.SendOrderRejectedEmail(
                order.AppUser.Email!,
                order.AppUser.UserName!
            );

            TempData["SuccessMessage"] =
                "Sipariş reddedildi ve kullanıcıya mail gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        // 🔸 Teslim durumu ayarla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDeliveryStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!Enum.TryParse(status, out DeliveryStatus deliveryStatus))
            {
                ModelState.AddModelError("", "Geçersiz teslim durumu.");
                return RedirectToAction(nameof(Index));
            }

            order.DeliveryStatus = deliveryStatus;
            await _context.SaveChangesAsync();

            // 🔥 Teslim edilmediyse ceza hesapla
            if (deliveryStatus == DeliveryStatus.NotDelivered)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                int lateDays = today.DayNumber - order.EndDate.DayNumber;
                if (lateDays < 0) lateDays = 0;

                decimal penaltyPerDay = 0;
                foreach (var item in order.OrderItems)
                {
                    penaltyPerDay = item.Car.DailyPrice * 2;
                    break;
                }

                decimal totalPenalty = penaltyPerDay * lateDays;

                string mailBody = $@"
Sayın {order.AppUser.UserName},

Kiralama süreniz {order.EndDate.ToString("dd.MM.yyyy")} tarihinde sona ermiştir
ancak aracı teslim etmediğiniz görülmektedir.

Geciken Gün Sayısı: {lateDays}
Günlük Ceza: {penaltyPerDay}₺
Toplam Ceza: {totalPenalty}₺

Lütfen en kısa sürede bizimle iletişime geçiniz.

İyi günler dileriz.
Emre Galeri";

                await _emailService.SendEmailAsync(
                    order.AppUser.Email!,
                    "Araç Teslim Uyarısı",
                    mailBody
                );
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
