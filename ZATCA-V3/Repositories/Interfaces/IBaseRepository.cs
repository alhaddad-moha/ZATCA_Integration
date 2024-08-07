using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<List<T>> GetAll();
    Task<T?> GetById(int id);
    Task Create(T entity);
    Task Update(T entity);
    Task Delete(int id);
    Task<bool> Exists(int id);

}