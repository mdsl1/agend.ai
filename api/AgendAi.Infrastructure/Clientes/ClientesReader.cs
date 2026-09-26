namespace AgendAi.Infrastructure.Clientes;

using AgendAi.Application.Clientes.Ports;
using AgendAi.Application.Clientes.Models;
using AgendAi.Domain.Clientes;
using NHibernate;
using NHibernate.Linq;

public sealed class ClientesReader : IClientesReader
{
    private readonly ISession _session;

    public ClientesReader(ISession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyCollection<DadosCliente>> ListarAsync(
        Guid clinicaUuid,
        CancellationToken cancellationToken
    )
    {
        return await _session
            .Query<Cliente>()
            .Where(cliente => 
                cliente.Clinica.Uuid == clinicaUuid
                && cliente.Clinica.DeletedAt == null
                && cliente.DeletedAt == null
            )
            .Select(cliente => new DadosCliente(
                cliente.Uuid,
                cliente.Nome,
                cliente.Telefone,
                cliente.Email,
                cliente.DataNascimento,
                cliente.Genero
            )).ToListAsync(cancellationToken);
    }
}