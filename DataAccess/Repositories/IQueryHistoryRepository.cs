// File: DataAccess/Repositories/IQueryHistoryRepository.cs
using SqlIdeProject.Models.Internal;
using System.Collections.Generic;

namespace SqlIdeProject.DataAccess.Repositories
{
    public interface IQueryHistoryRepository : IRepository<QueryHistory>
    {
        // Специфічний метод для отримання історії для конкретного профілю
        IEnumerable<QueryHistory> GetHistoryForProfile(int profileId);
    }
}