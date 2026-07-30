namespace AgendAi.Infrastructure;

using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using AgendAi.Infrastructure.Mappings;

public static class NHibernateHelper
{
    private static ISessionFactory? _sessionFactory;

    public static ISessionFactory CreateSessionFactory(string connectionString)
    {
        if (_sessionFactory != null) return _sessionFactory;

        _sessionFactory = Fluently.Configure()
            .Database(PostgreSQLConfiguration.PostgreSQL83
                .ConnectionString(connectionString)
                .Driver<NHibernate.Driver.NpgsqlDriver>())
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ClinicaMap>())
            .BuildSessionFactory();

        return _sessionFactory;
    }
}
