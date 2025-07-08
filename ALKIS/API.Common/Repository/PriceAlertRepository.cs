using API.Common.DTO;
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
    internal class PriceAlertRepository : IPriceAlertRepository
    {
        private readonly IEFConnector _connector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public PriceAlertRepository(IEFConnector connector, IMapper mapper, ILogger logger)
        {
            _connector = connector;
            _mapper = mapper;
            _logger = logger;
        }

        public bool UpdatePriceAlertLastMailSend(PriceAlert priceAlert)
        {
            try
            {
                var entityPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();

                var priceAlerts = entityPriceAlerts.GetAll();
                var priceAlertExists = priceAlerts.Where(p => p.Id == priceAlert.Id)?.FirstOrDefault();
                if (priceAlertExists != null)
                {
                    priceAlertExists.LastMailSend = priceAlert.LastMailSend;
                    entityPriceAlerts.Update(priceAlertExists);
                    _connector.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "ProductRepository", "UpdateProduct", ex);
                return false;
            }
            return true;
        }

        public bool UpdatePriceAlertConfirmed(int id, string email)
        {
            try
            {
                var entityPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();

                var priceAlerts = entityPriceAlerts.GetAll();
                var priceAlertExists = priceAlerts.Where(p => p.Id == id & p.EMail == email)?.FirstOrDefault();
                if (priceAlertExists != null)
                {
                    priceAlertExists.Confirmed = true;
                    entityPriceAlerts.Update(priceAlertExists);
                    _connector.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "ProductRepository", "UpdateProduct", ex);
                return false;
            }
            return true;
        }
        public bool InsertPriceAlert(PriceAlert priceAlert)
        {
            try
            {
                var entityPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();
                var entityProducts = _connector.GetRepository<API.Common.Entities.Product>();
                var product = entityProducts.Get(product => product.Name == priceAlert.ProductName);
                if (product != null)
                {
                    var priceAlerts = entityPriceAlerts.GetAll();
                    var priceAlertExists = priceAlerts.Where(p => p.EMail == priceAlert.EMail && p.ProductId == product.Id);
                    if (!priceAlertExists.Any())
                    {
                        priceAlert.ProductId = product.Id;
                        entityPriceAlerts.Insert(_mapper.Map<API.Common.Entities.PriceAlert>(priceAlert));
                        _connector.SaveChanges();
                        var newPriceAlert = entityPriceAlerts.Get(alert => alert.EMail == priceAlert.EMail && alert.ProductId == product.Id);
                        priceAlert.Id = newPriceAlert.Id;
                    }
                    else
                    {
                        priceAlert.Id = priceAlertExists.First().Id;
                    }
                }
                else
                {
                    _logger.LogError($"Produkt wurde in der Datenbank nicht gefunden {priceAlert.ProductName}!", APITypes.Common, "PriceAlertRepository", "InsertPriceAlert");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "PriceAlertRepository", "InsertPriceAlert", ex);
                return false;
            }
            return true;
        }
        public bool DeletePriceAlert(PriceAlert priceAlert)
        {
            try
            {
                var entityPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();
                var entityProducts = _connector.GetRepository<API.Common.Entities.Product>();
                var priceAlerts = entityPriceAlerts.GetAll();
                var product = entityProducts.Get(product => product.Name == priceAlert.ProductName);
                if (product != null)
                {
                    var priceAlertFound = priceAlerts.Where(p => p.EMail == priceAlert.EMail && p.ProductId == product.Id).FirstOrDefault();
                    if (priceAlertFound != null)
                    {
                        entityPriceAlerts.Delete(priceAlertFound);
                    }
                    _connector.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "PriceAlertRepository", "DeletePriceAlert", ex);
                return false;
            }
            return true;
        }

        public List<PriceAlert> GetPriceAlerts()
        {
            try
            {
                var reposPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();
                var entityProducts = _connector.GetRepository<API.Common.Entities.Product>().GetAll();
                var entityPriceAlerts = reposPriceAlerts.GetList(preisAlarm => preisAlarm.Confirmed == true);
                List<PriceAlert> dtoPriceAlerts = new List<PriceAlert>();
                //Für alle Entitäten Produktname mappen, da diese in der Datenbank bei PriceAlert nicht gibt um keine redundanz zu erzeugen
                foreach (var entityPriceAlert in entityPriceAlerts)
                {
                    var dtoPriceAlert = _mapper.Map<PriceAlert>(entityPriceAlert);
                    dtoPriceAlert.ProductName = entityProducts.Where(product => product.Id == entityPriceAlert.ProductId).FirstOrDefault().Name;
                    dtoPriceAlerts.Add(dtoPriceAlert);
                }

                return dtoPriceAlerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "PriceAlertRepository", "GetPriceAlert", ex);
                return null;
            }
        }
        public PriceAlert GetPriceAlert(PriceAlert priceAlert)
        {
            try
            {
                // PLZ, Radius, Email, Produktname
                var entityPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();
                var entityProducts = _connector.GetRepository<API.Common.Entities.Product>();
                var product = entityProducts.Get(product => product.Name == priceAlert.ProductName);
                var entityPriceAlert = entityPriceAlerts.Get(alert =>
                    (alert.PostCode == priceAlert.PostCode &&
                    alert.ProductId == product.Id && alert.EMail == priceAlert.EMail
                    && alert.Radius == priceAlert.Radius));
                if (entityPriceAlert != null)
                {
                    priceAlert.Price = entityPriceAlert.Price;
                }
                return priceAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "PriceAlertRepository", "GetPriceAlert", ex);
                return new PriceAlert();
            }
        }

        public List<PriceAlert> GetPriceAlertsByMail(string email)
        {
            List<PriceAlert> dtoPriceAlerts = new List<PriceAlert>();
            try
            {
                var reposPriceAlerts = _connector.GetRepository<API.Common.Entities.PriceAlert>();
                var entityProducts = _connector.GetRepository<API.Common.Entities.Product>().GetAll();
                var entityPriceAlerts = reposPriceAlerts.GetList(preisAlert => preisAlert.EMail == email);
                //Für alle Entitäten Produktname mappen, da diese in der Datenbank bei PriceAlert nicht gibt um keine redundanz zu erzeugen
                foreach (var entityPriceAlert in entityPriceAlerts)
                {
                    var dtoPriceAlert = _mapper.Map<PriceAlert>(entityPriceAlert);
                    dtoPriceAlert.ProductName = entityProducts.Where(product => product.Id == entityPriceAlert.ProductId).FirstOrDefault().Name;
                    dtoPriceAlerts.Add(dtoPriceAlert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "PriceAlertRepository", "GetPriceAlert", ex);
            }
            return dtoPriceAlerts;
        }
    }
}
