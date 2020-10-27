using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public interface IDatabaseRepository<T>
    {
        Task<T> CreateAsync(T model);
        Task<T> ReadAsync(int id);
        Task<T> UpdateAsync(T model);
        Task<bool> DeleteAsync(int id);
        Task<List<T>> ListAsync();
    }
}
