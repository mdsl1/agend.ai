using AgendAi.Application.Agenda.Services;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Infrastructure.Integracoes.N8n;
using AgendAi.Infrastructure.Agenda;
using AgendAi.Application.Agenda.ConsultarAgenda;
using AgendAi.Application.Agenda.ConsultarDisponibilidade;
using AgendAi.Application.Agenda.CriarAgendamento;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Infrastructure.Profissionais;
using AgendAi.Application.Profissionais.ListarProfissionais;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional;

using AgendAi.Application.Clientes.Ports;
using AgendAi.Infrastructure.Clientes;
using AgendAi.Application.Clientes.ResolverCliente;

using AgendAi.Infrastructure;
using AgendAi.API.Infrastructure;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Ports;

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
builder.Services.AddScoped<VerificarDisponibilidadeService>();

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

builder.Services.AddScoped<IAgendamentoAgendaReader, AgendamentoAgendaReader>();

builder.Services.AddScoped<IDisponibilidadeReader, DisponibilidadeReader>();
builder.Services.AddHttpClient<IDisponibilidadeExternaGateway, N8nDisponibilidadeGateway>(
    httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        httpClient.DefaultRequestHeaders.Add( "X-AgendAi-Api-Key", n8nApiKey );
    }
);
builder.Services.AddScoped<ConsultarDisponibilidadeHandler>();

builder.Services.AddScoped<ICriacaoAgendamentoReader, CriacaoAgendamentoReader>();
builder.Services.AddScoped<IAgendamentoWriter, AgendamentoWriter>();
builder.Services.AddHttpClient<
    ICriacaoAgendamentoExternoGateway,
    N8nCriacaoAgendamentoGateway
>(
    httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        httpClient.DefaultRequestHeaders.Add(
            "X-AgendAi-Api-Key",
            n8nApiKey
        );
    }
);
builder.Services.AddScoped<CriarAgendamentoHandler>();

builder.Services.AddScoped<IProfissionaisReader, ProfissionaisReader>();
builder.Services.AddScoped<ListarProfissionaisHandler>();

builder.Services.AddScoped<IProcedimentosProfissionalReader, ProcedimentosProfissionalReader>();
builder.Services.AddScoped<ListarProcedimentosProfissionalHandler>();

builder.Services.AddScoped<IResolucaoClienteReader, ResolucaoClienteReader>();
builder.Services.AddScoped<IClienteWriter, ClienteWriter>();
builder.Services.AddScoped<ResolverClienteHandler>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();
