// File: DataAccess/Repositories/IConnectionProfileRepository.cs
using SqlIdeProject.Models.Internal;

namespace SqlIdeProject.DataAccess.Repositories
{
    // Ми "успадковуємо" всі базові методи (GetById, GetAll, Add...)
    // і додаємо один новий, специфічний для профілів.
    public interface IConnectionProfileRepository : IRepository<ConnectionProfile>
    {
        // Наприклад, метод для пошуку профілю за його назвою
        ConnectionProfile? GetByName(string name);
    }
}