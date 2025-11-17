// File: DataAccess/Repositories/IRepository.cs
using System.Collections.Generic;

namespace SqlIdeProject.DataAccess.Repositories
{
    // T — це будь-який наш клас-модель (наприклад, ConnectionProfile)
    public interface IRepository<T> where T : class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}