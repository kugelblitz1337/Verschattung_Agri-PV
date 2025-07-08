using System.Runtime.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Common.DTO;
using API.Common.Enum;
using API.Common.Interfaces;

namespace API.Common.Caching
{
    public class CachingEngine : ICachingEngine
    {
        // Erstellen des Cache-Objekts
        private readonly MemoryCache _cache = MemoryCache.Default;

        public List<Product> GetProductsFromCache(APITypes apiType)
        {
            if (_cache.Where(cache => cache.Key.Contains(apiType.ToString())).ToList().Any())
            {
                return _cache.Where(cache => cache.Key.Contains(apiType.ToString())).Select(item => (Product)item.Value).ToList();
            }
            return new List<Product>();
        }

        public bool AddProductsToCache(APITypes apiType, Product product)
        {
            //Den Cache für 3 Tage halten
            return _cache.Add(apiType.ToString() + "_" + product.Name, product, DateTimeOffset.Now.AddDays(3));
        }
        public void DeleteProductsCache()
        {
            var entriesToRemove = _cache.Where(entry => entry.Value.GetType() == typeof(Product)).ToList();

            foreach (var entry in entriesToRemove)
            {
                _cache.Remove(entry.Key);
            }
        }
    }
}
