using System.Collections.Generic;
using System.Threading.Tasks;
using Pcf.Administration.Core.Domain;

namespace Pcf.Administration.Core.Abstractions.Repositories;

public interface IMongoRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task CreateAsync(T item);
    Task UpdateAsync(string id, T item);
    Task DeleteAsync(string id);
}
