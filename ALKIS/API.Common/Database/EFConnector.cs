using API.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Database
{
    /// <summary>
    /// Connector für das Entity Framework
    /// </summary>
    public class EFConnector : IEFConnector
    {
        private readonly DbContext _context;

        public EFConnector(DbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Repository für das Entity erstellen
        /// </summary>
        /// <typeparam name="TEntity">Entity das IEntity implementiert</typeparam>
        /// <returns>Repository Entity</returns>
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new Repository<TEntity>(_context);
        }

        /// <summary>
        /// Änderungen im Datenbankkontext speichern
        /// </summary>
        /// <returns>Datenbankeinträge geschrieben</returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Kontext auflösen
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
