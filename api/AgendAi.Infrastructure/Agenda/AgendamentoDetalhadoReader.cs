namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Domain.Agendamentos;
using NHibernate;
using NHibernate.Linq;

public sealed class AgendamentoDetalhadoReader : IAgendamentoDetalhadoReader
{
    private readonly ISession _session;

    public AgendamentoDetalhadoReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosAgendamentoDetalhado?> ConsultarAsync(
        Guid agendamentoUuid,
        Guid clinicaUuid,
        CancellationToken cancellationToken
    )
    {
        var agendamento = await _session
            .Query<Agendamento>()
            .Where(agendamento =>
                agendamento.Uuid == agendamentoUuid
                && agendamento.Clinica.Uuid == clinicaUuid
                && agendamento.DeletedAt == null
                && agendamento.Clinica.DeletedAt == null
            )
            .Select(agendamento => new {
                AgendamentoUuid = agendamento.Uuid,
                ClinicaUuid = agendamento.Clinica.Uuid,
                ClienteUuid = agendamento.Cliente.Uuid,
                NomeCliente = agendamento.Cliente.Nome,
                agendamento.Cliente.Telefone,
                ProfissionalUuid = agendamento.Profissional.Uuid,
                agendamento.Profissional.Prefixo, 
                NomeProfissional = agendamento.Profissional.Usuario.Nome,
                ProcedimentoUuid = agendamento.Procedimento.Uuid,
                NomeProcedimento = agendamento.Procedimento.Nome,
                agendamento.Procedimento.DuracaoEstimadaMinutos,
                agendamento.TimeDateInicio,
                agendamento.TimeDateFim,
                agendamento.MotivoContato,
                agendamento.AnotacoesProfissional,
                agendamento.ValorTotal,
                agendamento.Status,
                agendamento.StatusPagamento
            }).SingleOrDefaultAsync(cancellationToken);

        if (agendamento is null)
        {
            return null;
        }
        
        var nomeExibicaoProfissional = String.Concat(agendamento.Prefixo, " ", agendamento.NomeProfissional);

        return new DadosAgendamentoDetalhado(
            agendamento.AgendamentoUuid,
            agendamento.ClinicaUuid,
            agendamento.ClienteUuid,
            agendamento.NomeCliente,
            agendamento.Telefone,
            agendamento.ProfissionalUuid,
            nomeExibicaoProfissional,
            agendamento.ProcedimentoUuid,
            agendamento.NomeProcedimento,
            agendamento.DuracaoEstimadaMinutos,
            agendamento.TimeDateInicio,
            agendamento.TimeDateFim,
            agendamento.MotivoContato,
            agendamento.AnotacoesProfissional,
            agendamento.ValorTotal,
            agendamento.Status,
            agendamento.StatusPagamento
        );
    }
}