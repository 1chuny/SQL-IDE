// File: Models/Internal/UserSetting.cs
namespace SqlIdeProject.Models.Internal
{
    public class UserSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}