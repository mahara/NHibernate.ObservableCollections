using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;

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
                _sessionFactory ??= CreateSessionFactory();

                return _sessionFactory;
            }
        }

        private static ISessionFactory CreateSessionFactory()
        {
            var configuration = new Configuration();

            configuration.AddMapping(CreateMapping());

            return configuration.Configure().BuildSessionFactory();
        }

        private static HbmMapping? CreateMapping()
        {
            var mapper = new ModelMapper();

            mapper.Class<SampleItem>(
                c =>
                {
                    c.Id(x => x.Id,
                         m => m.Generator(Generators.Native));
                    c.Property(x => x.Name,
                               m => m.NotNullable(true));
                    c.ManyToOne(x => x.ParentSetContainer,
                                m =>
                                {
                                    m.Cascade(Cascade.Persist);
                                    m.Column("SetContainerId");
                                });
                });

            mapper.Class<SampleSetContainer>(
                c =>
                {
                    c.Id(x => x.Id,
                         m => m.Generator(Generators.Native));
                    c.Set(x => x.SampleSet,
                          m =>
                          {
                              //m.Lazy(CollectionLazy.NoLazy);
                              m.Type<ObservableSetType<SampleItem>>();
                              m.Inverse(true);
                              m.Key(k => k.Column("SetContainerID"));
                          },
                          r => r.OneToMany());
                });

            mapper.Class<SampleListContainer>(
                c =>
                {
                    c.Id(x => x.Id,
                         m => m.Generator(Generators.Native));
                    c.List(x => x.SampleList,
                           m =>
                           {
                               //m.Lazy(CollectionLazy.NoLazy);
                               m.Type<ObservableListType<SampleItem>>();
                               m.Table("Item_List");
                               m.Cascade(Cascade.Persist);
                               m.Key(k => k.Column("ListContainerId"));
                               m.Index(i => i.Column("PositionNumber"));
                           },
                           r => r.ManyToMany(m => m.Column("ItemId")));
                });

            var hbmMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            //var hbmMappingString = hbmMapping.AsString();

            return hbmMapping;
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
