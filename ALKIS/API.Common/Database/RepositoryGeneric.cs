using API.Common.Interfaces;
using Autofac;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace API.Common.Database
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        #region Löschen

        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(entity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die aus unserem Repository gelöscht werden soll.</param>
        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(entityList);</para>
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die aus unserem Repository gelöscht werden sollen.</param>
        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(p=> p.UserId == userId);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            _context.Set<TEntity>().AsQueryable()
                 .Where(predicate)
                 .ToList()
                 .ForEach(entity => _context.Set<TEntity>().Remove(entity));
        }

        #endregion

        #region Auswahl


        /// <summary>
        /// Wählt alle Entitäten aus unserem Repository aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.GetAll();</para>
        /// </summary>
        /// <returns>Gibt alle Entitäten aus unserem Repository zurück.</returns>
        public virtual IList<TEntity> GetAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        /// <summary>
        /// Wählt eine Entität aus unserem Repository unter Verwendung eines Filters aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.Get(p=> p.ProductId == 1);</para>
        /// <para>_repository.Get(p=> p.Name.Contains("Bier") == true);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Entität aus unserem Repository zurück.</returns>
        public virtual TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Wählt bestimmte Eigenschaften einer Entität aus unserem Repository aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.Get(p=> p.UserId == userId, p=> p.LastName);</para>
        /// </summary>
        /// <typeparam name="TResult">Von unserer Suche zurückgegebene Entität.</typeparam>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <param name="properties">Felder, die ausgewählt und in unserem Ergebnis befüllt werden sollen.</param>
        /// <returns>Gibt eine Entität aus unserem Repository zurück.</returns>
        public TResult Get<TResult>(Expression<Func<TEntity, bool>> predicate,
                                       Expression<Func<TEntity, TResult>> properties)
        {
            return _context.Set<TEntity>().Where(predicate)
                        .Select(properties)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Wählt eine Liste von Entitäten aus unserem Repository unter Verwendung eines Filters aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.GetList(p => p.LastName.Contains("Doe"));</para>
        /// <para>_repository.GetList(null);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Liste von Entitäten aus unserem Repository zurück.</returns>
        public IList<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate)
                        .ToList();
        }

        /// <summary>
        /// Wählt bestimmte Eigenschaften in der Liste von Entitäten aus unserem Repository unter Verwendung eines Filters aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.GetList(p => p.UserId, p => p.LastName.Contains("Doe"));</para>
        /// <para>_repository.GetList(p => p.UserId, p => p.IsAdmin == true);</para>
        /// </summary>
        /// <typeparam name="TResult">Von unserer Suche zurückgegebene Entität.</typeparam>
        /// <param name="properties">Felder, die ausgewählt und befüllt werden sollen.</param>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Liste von Entitäten aus unserem Repository zurück.</returns>
        public IList<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> predicate,
                                                   Expression<Func<TEntity, TResult>> properties)
        {
            return _context.Set<TEntity>().Where(predicate)
                        .Select(properties)
                        .ToList();
        }

        /// <summary>
        /// Methode zum Überprüfen, ob es Einträge in unserem Repository unter Verwendung eines Filters gibt.
        /// <para>Beispiele:</para>
        /// <para>_repository.Exists(p => p.UserId == user.Id)</para>
        /// <para>_repository.Exists(p => p.UserId == id &amp;&amp; p.IsAdmin == false);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt zurück, ob eine Entität anhand der Suchkriterien gefunden wurde.</returns>
        public virtual bool Exists(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Any(predicate);
        }

        #endregion
        #region Einfügen

        /// <summary>
        /// Fügt eine neue Entität unserem Repository hinzu.
        /// <para>Beispiele:</para>
        /// <para>_repository.Insert(newEntity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        public virtual void Insert(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// Methode zum Einfügen einer Liste von Entitäten in unser Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Insert(entityList);</para>
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die in unserem Repository gespeichert werden sollen.</param>
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        #endregion

        #region Aktualisieren

        /// <summary>
        /// Methode zum Aktualisieren einer einzelnen Entität.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(entity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        public virtual void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        /// <summary>
        /// Methode zum Aktualisieren unseres Repositorys unter Verwendung einer Liste von Entitäten.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(entityList);</para>
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die in unserem Repository gespeichert werden sollen.</param>
        public virtual void Update(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().UpdateRange(entities);
        }

        /// <summary>
        /// Methode zum Aktualisieren bestimmter Eigenschaften einer Entität.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(user, p => p.FirstName, p => p.LastName);</para>
        /// <para>_repository.Update(user, p => p.Password);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        /// <param name="properties">Array von Ausdrücken mit den Eigenschaften, die geändert werden sollen.</param>
        public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
        {
            _context.Attach(entity);

            foreach (var item in properties.AsParallel())
            {
                _context.Entry(entity).Property(item).IsModified = true;
            }
        }

        #endregion
    }
}