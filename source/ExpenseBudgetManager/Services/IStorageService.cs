using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Services
{
    public interface IStorageService
    {
        Task SaveAsync<T>(string fileName, T data);
        Task<T?> LoadAsync<T>(string fileName);
        Task DeleteAsync(string fileName);
        bool Exists(string fileName);
    }
}
