using API.Common.Authentification;
using API.Common.Caching;
using API.Common.Database;
using API.Common.Email;
using API.Common.Entities;
using API.Common.Interfaces;
using API.Common.LocationSearch;
using API.Common.Logger;
using API.Common.Repository;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace API.Common
{
    public class ModuleRegister : Autofac.Module
    {

        /// <summary>
        /// Registriert die Module der Common Komponente
        /// </summary>
        /// <param name="builder">IoC Container</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SupermarketSearch>().As<ISupermarketSearch>().SingleInstance();
            builder.RegisterType<ConsoleLogger>().As<ILogger>();
            builder.RegisterType<EMailService>().As<IEMailService>();
            builder.RegisterType<Database.DbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<EFConnector>().As<IEFConnector>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            builder.Register(c => new ResourceManager("API.Common.Properties.Resources", Assembly.GetExecutingAssembly()))
                   .As<ResourceManager>()
                   .InstancePerLifetimeScope();
            builder.RegisterType<MySqlConnectionStringBuilder>().As<MySqlConnectionStringBuilder>();
            //Automapper Konfigurationen hinzufügen
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DTO.Product, Product>();
                cfg.CreateMap<Product, DTO.Product>();
                cfg.CreateMap<DTO.PriceAlert, PriceAlert>();
                cfg.CreateMap<PriceAlert, DTO.PriceAlert>();
            });
            IMapper mapper = config.CreateMapper();
            builder.Register(m => mapper);
            builder.RegisterType<ProductRepository>().As<IProductRepository>();
            builder.RegisterType<PriceAlertRepository>().As<IPriceAlertRepository>();
            builder.RegisterType<AuthentificationHandler>().As<IAuthentificationHandler>();
            builder.RegisterType<CachingEngine>().As<ICachingEngine>();
        }
    }
}
