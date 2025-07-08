namespace API.Common.Interfaces
{
    /// <summary>
    /// Interface zum Entity Framework Connector
    /// </summary>
    public interface IEFConnector
    {
        /// <summary>
        /// Auslösen der Objekte
        /// </summary>
        void Dispose();

        /// <summary>
        /// Repository für das Entity erstellen
        /// </summary>
        /// <typeparam name="TEntity">Entity das IEntity implementiert</typeparam>
        /// <returns>Repository Entity</returns>
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        /// <summary>
        /// Änderungen im Datenbankkontext speichern
        /// </summary>
        /// <returns>Datenbankeinträge geschrieben</returns>
        int SaveChanges();
    }
}