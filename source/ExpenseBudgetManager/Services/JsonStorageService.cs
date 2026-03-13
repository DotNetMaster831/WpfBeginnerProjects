using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ExpenseBudgetManager.Services
{
    public class JsonStorageService : IStorageService
    {
        private readonly string _basePath;
        private readonly ILoggerService _logger;

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonStorageService(ILoggerService logger)
        {
            _logger = logger;
            _basePath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "ExpenseBudgetManager");

            Directory.CreateDirectory(_basePath);
            _logger.LogInformation($"Storage path: {_basePath}");
        }

        public async Task SaveAsync<T>(string fileName, T data)
        {
            try
            {
                var path = Path.Combine(_basePath, fileName);
                var json = JsonSerializer.Serialize(data, _options);
                await File.WriteAllTextAsync(path, json);
                _logger.LogDebug($"Saved: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save {fileName}", ex);
                throw;
            }
        }

        public async Task<T?> LoadAsync<T>(string fileName)
        {
            try
            {
                var path = Path.Combine(_basePath, fileName);
                if (!File.Exists(path))
                {
                    _logger.LogDebug($"File not found: {fileName}");
                    return default;
                }

                var json = await File.ReadAllTextAsync(path);
                var result = JsonSerializer.Deserialize<T>(json, _options);
                _logger.LogDebug($"Loaded: {fileName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load {fileName}", ex);
                return default;
            }
        }

        public async Task DeleteAsync(string fileName)
        {
            try
            {
                var path = Path.Combine(_basePath, fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.LogInformation($"Deleted: {fileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete {fileName}", ex);
                throw;
            }

            await Task.CompletedTask;
        }

        public bool Exists(string fileName)
        {
            var path = Path.Combine(_basePath, fileName);
            return File.Exists(path);
        }
    }
}
