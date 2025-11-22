// File: DataAccess/Repositories/IConnectionProfileRepository.cs
using SqlIdeProject.Models.Internal;

namespace SqlIdeProject.DataAccess.Repositories
{
    // Ми "успадковуємо" всі базові методи (GetById, GetAll, Add...)
    // і додаємо один новий, специфічний для профілів.
    public interface IConnectionProfileRepository : IRepository<ConnectionProfile>
    {
        ConnectionProfile? GetByName(string name);
    }
}