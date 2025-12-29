using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class ReminderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReminderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Tüm notlar (sadece giriş yapan kişinin)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // login'e yönlendir

            var reminders = await _context.Reminders
                .AsNoTracking()
                .Where(r => r.AppUserId == user.Id)
                .OrderBy(r => r.ReminderDate)
                .ToListAsync();

            return View(reminders);
        }

        // Yeni not oluşturma GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni not oluşturma POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReminderViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Eğer model.ReminderDate "local" geliyorsa UTC'ye çevirip kaydedelim.
            // (DB tarafını da timestamptz kullanacak şekilde standardize etmek en iyisi.)
            var reminderDateUtc = model.ReminderDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(model.ReminderDate, DateTimeKind.Local).ToUniversalTime()
                : model.ReminderDate.ToUniversalTime();

            var reminder = new Reminder
            {
                Title = model.Title?.Trim(),
                Description = model.Description?.Trim(),
                ReminderDate = reminderDateUtc,
                AppUserId = user.Id,
                IsCompleted = false
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Notu tamamlandı olarak işaretle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == user.Id);

            if (reminder == null) return NotFound();

            reminder.IsCompleted = !reminder.IsCompleted;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == user.Id);

            if (reminder == null) return NotFound();

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
