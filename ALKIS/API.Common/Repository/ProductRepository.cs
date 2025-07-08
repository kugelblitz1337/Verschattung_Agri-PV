using API.Common.DTO;
using API.Common.Entities;
using API.Common.Enum;
using API.Common.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Repository
{
    internal class ProductRepository : IProductRepository
    {
        private readonly IEFConnector _connector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProductRepository(IEFConnector connector, IMapper mapper, ILogger logger)
        {
            _connector = connector;
            _mapper = mapper;
            _logger = logger;
        }

        public bool InsertProduct(List<API.Common.DTO.Product> products)
        {
            try
            {
                var entityProducts = _connector.GetRepository<Entities.Product>();
                var productEntities = entityProducts.GetAll();
                //Alle Produkte ermitteln die es nicht als Entity gibt, denn diese müssen angelegt werden
                var productsNotInEntities = products
                    .Where(p => !productEntities.Select(pe => pe.Name).Contains(p.Name))
                    .ToList();
                foreach (var product in productsNotInEntities)
                {
                    entityProducts.Insert(_mapper.Map<Entities.Product>(product));
                }
                _connector.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "ProductRepository", "InsertProduct", ex);
                return false;
            }
            return true;
        }
        public DTO.Product GetProduct(string productName)
        {
            try
            {
                var entityProducts = _connector.GetRepository<Entities.Product>();

                var productsEntities = entityProducts.GetAll();
                var productEntity = productsEntities.Where(p => p.Name == productName).FirstOrDefault();
       
                return _mapper.Map<DTO.Product>(productEntity);
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "ProductRepository", "InsertProduct", ex);
                return null;
            }
        }
    }
}
