namespace AgendAi.Infrastructure.Clientes;

using AgendAi.Application.Clientes.Models;
using AgendAi.Application.Clientes.Ports;
using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;
using NHibernate;
using NHibernate.Linq;

public sealed class ResolucaoClienteReader : IResolucaoClienteReader
{
    private readonly ISession _session;

    public ResolucaoClienteReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosResolucaoCliente?> ObterAsync(
        Guid clinicaUuid,
        string telefone,
        string? telegramUserId,
        CancellationToken cancellationToken
    )
    {
        var clinica = await _session
            .Query<Clinica>()
            .SingleOrDefaultAsync(
                clinica => 
                    clinica.Uuid == clinicaUuid
                    && clinica.DeletedAt == null,
                cancellationToken
            );

        if (clinica is null)
        {
            return null;
        }

        var clientes = _session
            .Query<Cliente>()
            .Where(cliente => 
                cliente.Clinica.Id == clinica.Id
                && cliente.DeletedAt == null
            );
        
        var clientePorTelefone = await clientes
            .SingleOrDefaultAsync(
                cliente => cliente.Telefone == telefone,
                cancellationToken
            );

        Cliente? clientePorTelegram = null;

        if (!string.IsNullOrWhiteSpace(telegramUserId))
        {
            clientePorTelegram = await clientes
                .SingleOrDefaultAsync(
                    cliente => cliente.IdTelegram == telegramUserId,
                    cancellationToken
                );
        }

        return new DadosResolucaoCliente(
            Clinica: clinica,
            ClientePorTelefone: clientePorTelefone,
            ClientePorTelegram: clientePorTelegram
        );
    }
}