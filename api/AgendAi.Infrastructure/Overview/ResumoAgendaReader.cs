namespace AgendAi.Infrastructure.Overview;

using AgendAi.Application.Overview.Models;
using AgendAi.Application.Overview.Ports;
using AgendAi.Domain.Agendamentos;
using NHibernate;
using NHibernate.Linq;

public sealed class ResumoAgendaReader : IResumoAgendaReader
{
    private readonly ISession _session;

    public ResumoAgendaReader(
        ISession session
    )
    {
        _session = session;
    }

    public async Task<DadosResumoAgenda> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        DateTimeOffset inicioDia,
        DateTimeOffset fimDia,
        DateTimeOffset agora,
        CancellationToken cancellationToken
    )
    {
        var inicio = inicioDia.UtcDateTime;
        var fim = fimDia.UtcDateTime;

        var agendamentos = await _session
            .Query<Agendamento>()
            .Where(agendamento => 
                agendamento.Clinica.Uuid == clinicaUuid
                && agendamento.Profissional.Uuid == profissionalUuid
                && agendamento.Clinica.DeletedAt == null
                && agendamento.DeletedAt == null
                && agendamento.TimeDateInicio >= inicio
                && agendamento.TimeDateFim <= fim
            ).Select(agendamento => new
            {
                agendamento.Uuid,
                agendamento.TimeDateInicio,
                agendamento.TimeDateFim,
                NomeCliente = agendamento.Cliente.Nome,
                NomeProcedimento = agendamento.Procedimento.Nome,
                agendamento.Status,
                agendamento.StatusPagamento
            }).ToListAsync(cancellationToken);

        var qtdeHoje = agendamentos
            .Where(agendamento =>
                agendamento.StatusPagamento == Agendamento.StatusPagamentoPago
                || (agendamento.Status == Agendamento.StatusAgendado && agendamento.StatusPagamento == Agendamento.StatusPagamentoPendente)
            ).Count();

        var qtdeConfirmados = agendamentos
            .Where(agendamento => 
                agendamento.StatusPagamento == Agendamento.StatusPagamentoPago
            ).Count();
        
        var qtdeRestante = agendamentos
            .Where(agendamento =>
                agendamento.TimeDateInicio >= agora.UtcDateTime
            ).Count();

        var qtdeCancelados = agendamentos
            .Where(agendamento =>
                agendamento.Status == Agendamento.StatusCancelado
            ).Count();

        var proximoAgendamento = agendamentos
            .Where(agendamento => 
                agendamento.TimeDateInicio >= agora.UtcDateTime
                && agendamento.StatusPagamento == Agendamento.StatusPagamentoPago
                || (agendamento.Status == Agendamento.StatusAgendado && agendamento.StatusPagamento == Agendamento.StatusPagamentoPendente)
            ).OrderBy(agendamento => agendamento.TimeDateInicio)
            .Select(agendamento => new DadosProximoAgendamento(
                AgendamentoUuid: agendamento.Uuid,
                Inicio: agendamento.TimeDateInicio,
                Fim: agendamento.TimeDateFim,
                NomeCliente: agendamento.NomeCliente,
                NomeProcedimento: agendamento.NomeProcedimento,
                Status: agendamento.Status
            )).FirstOrDefault();
        
        return new DadosResumoAgenda(
            QtdeHoje: qtdeHoje,
            QtdeConfirmados: qtdeConfirmados,
            QtdeRestante: qtdeRestante,
            QtdeCancelados: qtdeCancelados,
            ProximoAgendamento: proximoAgendamento
        );
    }
}