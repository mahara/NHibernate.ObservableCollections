namespace NHibernate.ObservableCollections.DemoApp.DataAccess
{
    #region Using Directives

    using System;

    using NHibernate.Cfg;

    #endregion

    /// <summary>
    ///     Object encapsulating data access via NHibernate.
    /// </summary>
    public class NHibernateDbMgr : IDisposable
    {
        private static ISessionFactory _sessionFactory;

        private readonly ISession _session;

        private readonly ITransaction _transaction;

        public NHibernateDbMgr()
        {
            this._session = SessionFactory.OpenSession(); // open new session
            this._transaction = this._session.BeginTransaction();
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
                this._transaction.Commit();

                this._transaction.Dispose();
            }
            catch
            {
                try
                {
                    this._transaction.Rollback();
                }
                catch
                {
                }
            }
            finally
            {
                this._session.Close();

                this._session.Dispose();
            }
        }

        /// <summary>
        ///     Delete an object from the database.
        /// </summary>
        public void Delete(object o)
        {
            this._session.Delete(o);
            this._session.Flush();
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
            return this._session.Get<T>(id);
        }

        /// <summary>
        ///     Save a new object to the database.
        /// </summary>
        public void Save(object o)
        {
            this._session.Save(o);
            this._session.Flush();
        }

        /// <summary>
        ///     Updates an object that already exists within the database.
        /// </summary>
        public void Update(object o)
        {
            this._session.Update(o);
            this._session.Flush();
        }
    }
}