namespace NHibernate.ObservableCollections.DemoApp.DataAccess
{
    using NHibernate.Cfg;

    /// <summary>
    ///     Object encapsulating data access via NHibernate.
    /// </summary>
    public class NHibernateDbMgr : IDisposable
    {
        private static ISessionFactory? _sessionFactory;

        private readonly ISession _session;

        private readonly ITransaction _transaction;

        public NHibernateDbMgr()
        {
            _session = SessionFactory.OpenSession(); // open new session
            _transaction = _session.BeginTransaction();
        }

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = new Configuration().Configure().BuildSessionFactory();
                }

                return _sessionFactory;
            }
        }

        public void Dispose()
        {
            try
            {
                _transaction.Commit();

                _transaction.Dispose();
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
                _session.Close();

                _session.Dispose();
            }
        }

        /// <summary>
        ///     Delete an object from the database.
        /// </summary>
        public void Delete(object o)
        {
            _session.Delete(o);
            _session.Flush();
        }

        /// <summary>
        ///     Loads the specified object from the database.
        /// </summary>
        /// <typeparam name="T">Type of object to load</typeparam>
        /// <param name="id">Identifier of object to load</param>
        /// <returns>
        ///     Object as specified from the database; otherwise NULL if
        ///     the object could not be found.
        /// </returns>
        public T Get<T>(object id)
        {
            return _session.Get<T>(id);
        }

        /// <summary>
        ///     Save a new object to the database.
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
    }
}
