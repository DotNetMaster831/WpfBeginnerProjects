using ExpenseBudgetManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Services
{
    public class JsonTransactionStore : ITransactionStore
    {
        private readonly IStorageService _storage;
        private readonly ILoggerService _logger;
        private const string FileName = "transactions.json";

        private List<Transaction> _cache = new();

        public JsonTransactionStore(IStorageService storage,
            ILoggerService logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            if (_cache.Count > 0) return _cache;

            var data = await _storage.LoadAsync<List<Transaction>>(FileName);
            _cache = data ?? new List<Transaction>();

            _logger.LogInformation($"Loaded {_cache.Count} transactions.");
            return _cache;
        }

        public async Task AddAsync(Transaction transaction)
        {
            _cache.Add(transaction);
            await SaveAsync();
            _logger.LogInformation($"Added transaction: {transaction.Title}");
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = _cache.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                _cache.Remove(item);
                await SaveAsync();
                _logger.LogInformation($"Deleted transaction: {id}");
            }
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            var index = _cache.FindIndex(t => t.Id == transaction.Id);
            if (index >= 0)
            {
                _cache[index] = transaction;
                await SaveAsync();
                _logger.LogInformation($"Updated transaction: {transaction.Title}");
            }
        }

        public async Task SaveAsync()
        {
            await _storage.SaveAsync(FileName, _cache);
        }
    }
}
