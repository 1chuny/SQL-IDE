namespace SqlIdeProject.DataAccess.Repositories
{
    public interface IUserSettingsRepository
    {
        // Отримати налаштування за ключем
        string? GetSetting(string key);

        // Зберегти налаштування (додати або оновити)
        void SaveSetting(string key, string value);
    }
}