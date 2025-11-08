using AmongClass.Models;

namespace AmongClass.IRepository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category> GetCategoryByIdAsync(Guid id);
        Task<Category> CreateCategory(Category category);
        Task<Category> UpdateCategory(Category category);
        Task<bool> DeleteCategory(Guid id);
    }
}
