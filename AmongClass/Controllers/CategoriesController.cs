using AmongClass.IRepository;
using AmongClass.Models;
using Microsoft.AspNetCore.Mvc;

namespace AmongClass.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _catRepo;

        public CategoriesController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }

        // GET: /Categories
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _catRepo.GetAllCategories();
            return View(categories);
        }

        // GET: /Categories/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _catRepo.CreateCategory(model);
            TempData["SuccessMessage"] = "Categoria a fost creată cu succes.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _catRepo.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _catRepo.UpdateCategory(model);
                TempData["SuccessMessage"] = "Categoria a fost actualizată.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Categories/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _catRepo.DeleteCategory(id);
            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "Categoria a fost ștearsă.";
            return RedirectToAction(nameof(Index));
        }
    }
}
