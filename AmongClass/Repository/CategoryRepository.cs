using AmongClass.Data;
using AmongClass.IRepository;
using AmongClass.Models;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> CreateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.Id = Guid.NewGuid();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Category with Id {category.Id} not found.");

            _context.Entry(existing).CurrentValues.SetValues(category);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
