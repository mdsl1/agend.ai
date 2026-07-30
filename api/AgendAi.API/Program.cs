using AgendAi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

builder.Services.AddSingleton(_ => NHibernateHelper.CreateSessionFactory(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<NHibernate.ISessionFactory>().OpenSession());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
