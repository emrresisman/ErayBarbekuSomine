using Microsoft.AspNetCore.Mvc;
using ErayBarbekuSomine.Models;
using Microsoft.EntityFrameworkCore;

namespace ErayBarbekuSomine.Controllers
{
	public class ProductsController : Controller
	{
		private readonly AppDbContext _context;

		public ProductsController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> DuzCepheliSomine()
		{
			var images = await _context.Images.Where(i => i.Category == "DuzCepheliSomine").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
		public async Task<IActionResult> KoseSomine()
		{
			var images = await _context.Images.Where(i => i.Category == "KoseSomine").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
		public async Task<IActionResult> LTipiSomine()
		{
			var images = await _context.Images.Where(i => i.Category == "LTipiSomine").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}

		public async Task<IActionResult> TruvaBarbeku()
		{
			var images = await _context.Images.Where(i => i.Category == "TruvaBarbeku").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
		public async Task<IActionResult> TruvaCiftTarafli()
		{
			var images = await _context.Images.Where(i => i.Category == "TruvaCiftTarafli").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
		public async Task<IActionResult> TruvaTekTarafli()
		{
			var images = await _context.Images.Where(i => i.Category == "TruvaTekTarafli").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
		public async Task<IActionResult> UTipiSomine()
		{
			var images = await _context.Images.Where(i => i.Category == "UTipiSomine").OrderByDescending(i => i.UploadDate).ToListAsync();
			return View(images);
		}
	}
}
