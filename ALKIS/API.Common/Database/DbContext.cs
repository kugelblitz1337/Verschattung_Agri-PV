using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Reflection.Emit;
using API.Common.Entities;
using System.Resources;
using MySqlX.XDevAPI;
using System.Reflection;
using API.Common.Interfaces;

namespace API.Common.Database
{
    /// <summary>
    /// Datenbank Kontext aufbauen mit DB Verbindung und generierung der Datenmodelle
    /// </summary>
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly ResourceManager _resourceManager;
        private readonly MySqlConnectionStringBuilder _mySqlConnectionStringBuilder;

        public DbContext(ResourceManager resourceManager, MySqlConnectionStringBuilder mySqlConnectionStringBuilder)
        {
            _resourceManager = resourceManager;
            _mySqlConnectionStringBuilder = mySqlConnectionStringBuilder;
        }

        /// <summary>
        /// MySQL Aufbau mit ConnectionString
        /// </summary>
        /// <param name="optionsBuilder">Datenbank Optionsbuilder für setzen des Connectionsstrings</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(GetConnectionString());
        }
        /// <summary>
        /// Alle Modelle die IEntity implementieren erstellen
        /// </summary>
        /// <param name="modelBuilder">Modellbuilder um Entities zu registrieren</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Alle Entitätsklassen im Assembly finden
            var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity).IsAssignableFrom(t));

            // Entitätsklassen dem Kontext hinzufügen
            foreach (var entityType in entityTypes)
            {
                var entityBuilder = modelBuilder.Entity(entityType);

                // Alle Eigenschaften durchgehen und nullable Eigenschaften behandeln
                foreach (var property in entityType.GetProperties())
                {
                    if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        // Nullable-Eigenschaften als nullable konfigurieren
                        entityBuilder.Property(property.Name).IsRequired(false);
                    }
                }
            }
        }

        /// <summary>
        /// Erstellen des Connectionsstrings
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            //Setzen der Properties
            _mySqlConnectionStringBuilder.Database = _resourceManager.GetString("DBName");
            _mySqlConnectionStringBuilder.UserID = _resourceManager.GetString("User");
            _mySqlConnectionStringBuilder.Password = _resourceManager.GetString("UserPW");
            _mySqlConnectionStringBuilder.Server = _resourceManager.GetString("Server");
            _mySqlConnectionStringBuilder.Port = Convert.ToUInt32(_resourceManager.GetString("Port"));

            return _mySqlConnectionStringBuilder.ToString();
        }
    }
}
