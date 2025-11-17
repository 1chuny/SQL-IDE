// File: Models/Internal/ConnectionProfile.cs
using System;

namespace SqlIdeProject.Models.Internal
{
    public class ConnectionProfile
    {
        public int Id { get; set; }
        public string ProfileName { get; set; }
        public string DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        public DateTime LastUsed { get; set; }
    }
}