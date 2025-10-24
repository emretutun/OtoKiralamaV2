using System.Diagnostics;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EmreGaleriApp.Web.Extensions;
using EmreGaleriApp.Web.Services;
using Microsoft.EntityFrameworkCore;
using EmreGaleriApp.Service.Services;

namespace EmreGaleriApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _dbContext;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _dbContext.Cars.Where(c => c.IsAvailable).ToListAsync();
            return View(cars);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        // ✅ Kullanıcı kayıt ve e-posta onay linki gönderme
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email!.Trim().ToLowerInvariant());
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                return View(request);
            }

            var newUser = new AppUser
            {
                UserName = request.UserName,
                PhoneNumber = request.Phone,
                Email = request.Email
            };

            var identityResult = await _userManager.CreateAsync(newUser, request.Password!);

            if (identityResult.Succeeded)
            {
                // ✅ Email onay token oluştur
                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                // ✅ Onay linki oluştur
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Home",
                    new { userId = newUser.Id, token = emailConfirmationToken },
                    HttpContext.Request.Scheme,
                    HttpContext.Request.Host.Value
                );

                // ✅ Onay mailini gönder
                // ✅ Onay mailini gönder
                await _emailService.SendEmailAsync(
                    newUser.Email!,
                    "E-posta Onayı - Emre Galeri",
                    $@"
Merhaba {newUser.UserName},

Emre Galeri hesabınızı aktif hale getirmek için e-posta adresinizi onaylamanız gerekmektedir.

Hesabınızı onaylamak için aşağıdaki bağlantıya tıklayınız:

{confirmationLink}

Bu bir otomatik e-postadır, lütfen yanıtlamayınız.
Eğer bu işlemi siz başlatmadıysanız, bu e-postayı dikkate almayınız.

Saygılarımızla,
Emre Galeri Ekibi
"
                );


                TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen e-posta adresinizi onaylayın.";
                return RedirectToAction(nameof(SignUp));
            }

            ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
            return View(request);
        }

        // ✅ Kullanıcının e-posta onayını işleyen action
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData["ErrorMessage"] = "Geçersiz onay isteği.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "E-posta adresiniz başarıyla onaylandı. Giriş yapabilirsiniz.";
                return RedirectToAction("SignIn");
            }

            TempData["ErrorMessage"] = "E-posta onayı başarısız oldu.";
            return RedirectToAction("Index");
        }

        public IActionResult SignIn()
        {
            return View();
        }

        // ✅ Giriş yaparken e-posta onayı kontrolü
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Mail veya şifre yanlış!");
                return View(model);
            }

            // ✅ E-posta onay kontrolü
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Lütfen e-posta adresinizi onaylayın.");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Mail veya şifre yanlış!");
            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel request)
        {
            var hasUser = await _userManager.FindByEmailAsync(request.Email!);

            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Mail bulunamadı!");
                return View();
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

            var passwordResetLink = Url.Action(
                "ResetPassword",
                "Home",
                new { userId = hasUser.Id, token = passwordResetToken },
                HttpContext.Request.Scheme,
                HttpContext.Request.Host.Value
            );

            await _emailService.SendResetPasswordEmail(passwordResetLink!, hasUser!.Email!);

            TempData["SuccessMessage"] = "Şifre sıfırlama maili gönderildi.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı veya token.";
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model!.UserId!);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, model.Token!, model.Password!);

            if (resetResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı.";
                return RedirectToAction("SignIn");
            }
            else
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
