using ErayBarbekuSomine.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ErayBarbekuSomine.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            string[] categories = { "DuzCepheliSomine", "KoseSomine", "LTipiSomine", "UTipiSomine", "TruvaBarbeku", "TruvaCiftTarafli", "TruvaTekTarafli" };
            var d = new Dictionary<string, Image>();
            foreach(var c in categories) {
                var img = _context.Images.OrderByDescending(i => i.UploadDate).FirstOrDefault(i => i.Category == c);
                if(img != null) {
                    d[c] = img;
                }
            }
            return View(d);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContactMessage(ContactMessage model, string honeypot)
        {
            try
            {
                // Honeypot Anti-Spam: If this hidden field is filled, it's a bot.
                if (!string.IsNullOrEmpty(honeypot))
                {
                    // Pretend to succeed to fool the bot, but do not save.
                    return Json(new { success = true, message = "Mesajınız başarıyla gönderildi!" });
                }

                ModelState.Remove("Id");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("IsRead");
                ModelState.Remove("honeypot");

                if (ModelState.IsValid)
                {
                    model.CreatedAt = DateTime.Now;
                    model.IsRead = false;
                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Mesajınız bize ulaştı. En kısa sürede dönüş yapacağız." });
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Lütfen alanları doğru doldurun. Hata: " + string.Join(" | ", errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İletişim formu kaydedilirken hata oluştu.");
                return Json(new { success = false, message = "Sistemsel bir hata oluştu, lütfen daha sonra tekrar deneyiniz." });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}