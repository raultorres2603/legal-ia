using System;
using System.Threading.Tasks;

namespace Legal_IA.Interfaces.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task RemoveByPatternAsync(string pattern);
    }
}

