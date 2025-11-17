// File: Models/Internal/QueryHistory.cs
using System;

namespace SqlIdeProject.Models.Internal
{
    public class QueryHistory
    {
        public int Id { get; set; }
        public string QueryText { get; set; }
        public DateTime ExecutionTime { get; set; }
        public bool WasSuccessful { get; set; }
        
        // Зовнішній ключ, щоб ми знали, до якого профілю належить цей запит
        public int? ConnectionProfileId { get; set; }
    }
}