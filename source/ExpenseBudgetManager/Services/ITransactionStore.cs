using ExpenseBudgetManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Services
{
    public interface ITransactionStore
    {
        Task<List<Transaction>> GetAllAsync();
        Task AddAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(Transaction transaction);
        Task SaveAsync();
    }
}
