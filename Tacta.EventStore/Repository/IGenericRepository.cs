using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IGenericRepository
    {
        Task<T> GetAsync<T>(Guid id);
        Task<IEnumerable<T>> GetAllAsync<T>();
        Task InsertAsync<T>(T t);
        Task<int> InsertAsync<T>(IEnumerable<T> list);
        Task UpdateAsync<T>(T t);
        Task DeleteAsync(Guid id);
        Task DeleteAllAsync();
    }
}