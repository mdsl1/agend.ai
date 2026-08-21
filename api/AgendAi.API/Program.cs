using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.ConsultarAgenda;
using AgendAi.Infrastructure;
using AgendAi.Infrastructure.Agenda;
using AgendAi.Infrastructure.Integracoes.N8n;
using AgendAi.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

var n8nApiKey = builder.Configuration["N8n:ApiKey"];
if (string.IsNullOrWhiteSpace(n8nApiKey))
{
    throw new InvalidOperationException(
        "A configuração 'N8n:ApiKey' não foi encontrada."
    );
}

builder.Services.AddSingleton(_ => NHibernateHelper.CreateSessionFactory(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<NHibernate.ISessionFactory>().OpenSession());

builder.Services.AddScoped< IProfissionalAgendaReader, ProfissionalAgendaReader >();
builder.Services.AddHttpClient<
    IAgendaExternaGateway,
    N8nAgendaGateway>(
        httpClient =>
        {
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add( "X-AgendAi-Api-Key", n8nApiKey );
        });

builder.Services.AddScoped<ConsultarAgendaHandler>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();