using ErayBarbekuSomine.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ErayBarbekuSomine.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var adminUser = _configuration["AdminLogin:Username"];
            var adminPass = _configuration["AdminLogin:Password"];

            if (username == adminUser && password == adminPass)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });

                return RedirectToAction("Index");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewBag.ImageCount = _context.Images.Count();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Gallery()
        {
            var images = await _context.Images.OrderBy(i => i.Category).ThenByDescending(i => i.UploadDate).ToListAsync();
            return View(images);
        }

        [Authorize]
        public IActionResult Upload()
        {
            var categories = new[] { "DuzCepheliSomine", "KoseSomine", "LTipiSomine", "TruvaBarbeku", "TruvaCiftTarafli", "TruvaTekTarafli", "UTipiSomine" };
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, string category)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });

            if (string.IsNullOrEmpty(category))
                return Json(new { success = false, message = "Lütfen kategori seçin." });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", category);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var image = new Image
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{category}/{uniqueFileName}",
                Category = category,
                UploadDate = DateTime.Now
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true, filePath = image.FilePath, id = image.Id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReplaceImage(int id, IFormFile file, string category)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });

            var image = await _context.Images.FindAsync(id);
            if (image == null) return Json(new { success = false, message = "Resim bulunamadı." });

            // File deletion code removed here as requested by the user.

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", category);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Veritabanını güncelle
            image.FileName = file.FileName;
            image.FilePath = $"/images/{category}/{uniqueFileName}";
            image.UploadDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true, newUrl = image.FilePath });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategoryCover(string category, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", category.Replace("ı", "i").ToLower()); // Just a safe fallback folder path
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var relativePath = $"/images/{category.Replace("ı", "i").ToLower()}/{uniqueFileName}";

            var existingImage = await _context.Images.FirstOrDefaultAsync(i => i.Category == category);
            if (existingImage != null)
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, existingImage.FilePath.TrimStart('/'));
                // if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                
                existingImage.FileName = file.FileName;
                existingImage.FilePath = relativePath;
                existingImage.UploadDate = DateTime.Now;
            }
            else
            {
                _context.Images.Add(new Image
                {
                    FileName = file.FileName,
                    FilePath = relativePath,
                    Category = category,
                    UploadDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, newUrl = relativePath });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return Json(new { success = false, message = "Resim bulunamadı." });

            var filePath = Path.Combine(_env.WebRootPath, image.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCategoryCover(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return Json(new { success = false, message = "Resim bulunamadı." });

            // To set it as cover, we just make it the most recently uploaded/edited image for this category.
            // This avoids needing a database schema change.
            image.UploadDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteImages(List<int> ids)
        {
            if (ids == null || !ids.Any()) return Json(new { success = false, message = "Silinek resim seçilmedi." });

            var images = await _context.Images.Where(i => ids.Contains(i.Id)).ToListAsync();
            
            foreach(var image in images)
            {
                var filePath = Path.Combine(_env.WebRootPath, image.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Images.RemoveRange(images);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // --- MESSAGE MANAGEMENT ---

        [Authorize]
        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View(messages);
        }

        [Authorize]
        public async Task<IActionResult> MessageDetail(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                _context.Update(message);
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return Json(new { success = false, message = "Mesaj bulunamadı." });

            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Mesaj silindi." });
        }
    }
}
