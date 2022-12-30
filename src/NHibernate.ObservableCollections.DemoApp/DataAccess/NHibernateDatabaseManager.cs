using NHibernate.Cfg;

namespace NHibernate.ObservableCollections.DemoApp.DataAccess
{
    /// <summary>
    ///     Object encapsulating data access via NHibernate.
    /// </summary>
    public class NHibernateDatabaseManager : IDisposable
    {
        private static ISessionFactory? _sessionFactory;

        private readonly ISession _session;

        private readonly ITransaction _transaction;

        public NHibernateDatabaseManager()
        {
            _session = SessionFactory.OpenSession(); // Open a new session.
            _transaction = _session.BeginTransaction();
        }

        public void Dispose()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                try
                {
                    _transaction.Rollback();
                }
                catch
                {
                }
            }
            finally
            {
                _transaction.Dispose();

                _session.Close();
                _session.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private static ISessionFactory SessionFactory
        {
            get
            {
                _sessionFactory ??= new Configuration().Configure().BuildSessionFactory();

                return _sessionFactory;
            }
        }

        /// <summary>
        ///     Loads the specified object from the database.
        /// </summary>
        /// <typeparam name="T">Type of object to load.</typeparam>
        /// <param name="id">Identifier of object to load.</param>
        /// <returns>
        ///     Object as specified from the database;
        ///     otherwise <see langword="null" /> if the object could not be found.
        /// </returns>
        public T Get<T>(object id)
        {
            return _session.Get<T>(id);
        }

        /// <summary>
        ///     Saves a new object to the database.
        /// </summary>
        public void Save(object o)
        {
            _session.Save(o);
            _session.Flush();
        }

        /// <summary>
        ///     Updates an object that already exists within the database.
        /// </summary>
        public void Update(object o)
        {
            _session.Update(o);
            _session.Flush();
        }

        /// <summary>
        ///     Deletes an object from the database.
        /// </summary>
        public void Delete(object o)
        {
            _session.Delete(o);
            _session.Flush();
        }
    }
}
