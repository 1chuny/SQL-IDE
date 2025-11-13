// File: Services/QueryCompletedEventArgs.cs
using System;
using System.Data;

// Клас для передачі даних події
public class QueryCompletedEventArgs : EventArgs
{
    public DataTable? Result { get; }
    public Exception? Error { get; }
    public bool IsSuccess => Error == null;

    // Конструктор для успішного результату
    public QueryCompletedEventArgs(DataTable result)
    {
        Result = result;
        Error = null;
    }

    // Конструктор для помилки
    public QueryCompletedEventArgs(Exception error)
    {
        Result = null;
        Error = error;
    }
}