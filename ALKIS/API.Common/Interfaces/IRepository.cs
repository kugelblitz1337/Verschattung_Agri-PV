using System.Linq.Expressions;

namespace API.Common.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(p=> p.UserId == userId);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        void Delete(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(entityList);</para>
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die aus unserem Repository gelöscht werden sollen.</param>
        void Delete(IEnumerable<TEntity> entities);
        /// <summary>
        /// Löscht eine Entität aus unserem Repository.
        /// <para>Beispiele:</para>
        /// <para>_repository.Delete(entity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die aus unserem Repository gelöscht werden soll.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Methode zum Überprüfen, ob es Einträge in unserem Repository unter Verwendung eines Filters gibt.
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt zurück, ob eine Entität anhand der Suchkriterien gefunden wurde.</returns>
        bool Exists(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// Methode zum Einfügen einer Liste von Entitäten in unser Repository.
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die in unserem Repository gespeichert werden sollen.</param>
        void Insert(IEnumerable<TEntity> entities);
        /// <summary>
        /// Fügt eine neue Entität unserem Repository hinzu.
        /// <para>Beispiele:</para>
        /// <para>_repository.Insert(newEntity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        void Insert(TEntity entity);
        /// <summary>
        /// Wählt eine Entität aus unserem Repository unter Verwendung eines Filters aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.Get(p=> p.ProductId == 1);</para>
        /// <para>_repository.Get(p=> p.Name.Contains("Bier") == true);</para>
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Entität aus unserem Repository zurück.</returns>
        TEntity Get(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Wählt bestimmte Eigenschaften einer Entität aus unserem Repository aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.Get(p=> p.UserId == userId, p=> p.LastName);</para>
        /// </summary>
        /// <typeparam name="TResult">Von unserer Suche zurückgegebene Entität.</typeparam>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <param name="properties">Felder, die ausgewählt und in unserem Ergebnis befüllt werden sollen.</param>
        /// <returns>Gibt eine Entität aus unserem Repository zurück.</returns>
        TResult Get<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> properties);

        /// <summary>
        /// Wählt alle Entitäten aus unserem Repository aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.GetAll();</para>
        /// </summary>
        /// <returns>Gibt alle Entitäten aus unserem Repository zurück.</returns>
        IList<TEntity> GetAll();
        /// <summary>
        /// Wählt eine Liste von Entitäten aus unserem Repository unter Verwendung eines Filters aus.
        /// </summary>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Liste von Entitäten aus unserem Repository zurück.</returns>
        IList<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// Wählt bestimmte Eigenschaften in der Liste von Entitäten aus unserem Repository unter Verwendung eines Filters aus.
        /// <para>Beispiele:</para>
        /// <para>_repository.GetList(p => p.UserId, p => p.LastName.Contains("Doe"));</para>
        /// </summary>
        /// <typeparam name="TResult">Von unserer Suche zurückgegebene Entität.</typeparam>
        /// <param name="properties">Felder, die ausgewählt und befüllt werden sollen.</param>
        /// <param name="predicate">Filter, der auf unsere Suche angewendet wird.</param>
        /// <returns>Gibt eine Liste von Entitäten aus unserem Repository zurück.</returns>
        IList<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> properties);
        /// <summary>
        /// Methode zum Aktualisieren unseres Repositorys unter Verwendung einer Liste von Entitäten.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(entityList);</para>
        /// </summary>
        /// <param name="entities">Liste von Entitäten, die in unserem Repository gespeichert werden sollen.</param>
        void Update(IEnumerable<TEntity> entities);
        /// <summary>
        /// Methode zum Aktualisieren einer einzelnen Entität.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(entity);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        void Update(TEntity entity);
        /// <summary>
        /// Methode zum Aktualisieren bestimmter Eigenschaften einer Entität.
        /// <para>Beispiele:</para>
        /// <para>_repository.Update(user, p => p.FirstName, p => p.LastName);</para>
        /// </summary>
        /// <param name="entity">Entitätsinstanz, die in unserem Repository gespeichert werden soll.</param>
        /// <param name="properties">Array von Ausdrücken mit den Eigenschaften, die geändert werden sollen.</param>
        void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties);
    }
}