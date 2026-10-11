namespace AgendAi.Infrastructure.Receitas;

using AgendAi.Application.Receitas.Models;
using AgendAi.Application.Receitas.Ports;
using AgendAi.Domain.Agendamentos;
using NHibernate;
using NHibernate.Linq;

public sealed class ResumoMinhasReceitasReader : IResumoMinhasReceitasReader
{
    private readonly ISession _session;

    public ResumoMinhasReceitasReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosResumoMinhasReceitas> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        DateTimeOffset inicioMesAnterior,
        DateTimeOffset inicioMes,
        DateTimeOffset inicioProximoMes,
        CancellationToken cancellationToken
    )
    {
        var inicioAnterior = inicioMesAnterior.UtcDateTime;
        var inicio = inicioMes.UtcDateTime;
        var inicioProximo = inicioProximoMes.UtcDateTime;

        var registros = await _session
            .Query<Agendamento>()
            .Where(agendamento =>
                agendamento.Clinica.Uuid == clinicaUuid
                && agendamento.Profissional.Uuid == profissionalUuid
                && agendamento.Clinica.DeletedAt == null
                && agendamento.DeletedAt == null
                && agendamento.TimeDateInicio >= inicioAnterior
                && agendamento.TimeDateInicio < inicioProximo
            ).Select(agendamento => new
            {
                agendamento.TimeDateInicio,
                agendamento.ValorTotal,
                agendamento.Status,
                agendamento.StatusPagamento,
                ProcedimentoUuid = agendamento.Procedimento.Uuid,
                NomeProcedimento = agendamento.Procedimento.Nome
            }).ToListAsync(cancellationToken);

        var registrosMesAtual = registros
            .Where(registro =>
                registro.TimeDateInicio >= inicio
                && registro.TimeDateInicio < inicioProximo
            ).ToArray();

        var receitaRecebida = registrosMesAtual
            .Where(registro => 
                registro.StatusPagamento == Agendamento.StatusPagamentoPago
            ).Sum(registro => registro.ValorTotal);
        
        var receitaEstimada = registrosMesAtual
            .Where(registro => 
                registro.Status == Agendamento.StatusAgendado
                && registro.StatusPagamento == Agendamento.StatusPagamentoPendente
            ).Sum(registro => registro.ValorTotal);
        
        var receitaRecebidaMesAnterior = registros
            .Where(registro =>
                registro.TimeDateInicio >= inicioAnterior
                && registro.TimeDateInicio < inicio
                && registro.StatusPagamento == Agendamento.StatusPagamentoPago
            ).Sum(registro => registro.ValorTotal);

        var receitaPorProcedimento = registrosMesAtual
            .Where(registro =>
                registro.StatusPagamento == Agendamento.StatusPagamentoPago
                || (registro.Status == Agendamento.StatusAgendado && registro.StatusPagamento == Agendamento.StatusPagamentoPendente)
            ).GroupBy(registro => new
            {
                registro.ProcedimentoUuid,
                registro.NomeProcedimento
            })
            .Select(grupo => new DadosMinhasReceitasPorProcedimento(
                ProcedimentoUuid: grupo.Key.ProcedimentoUuid,
                Nome: grupo.Key.NomeProcedimento,
                ValorTotal: grupo.Sum(registro => registro.ValorTotal),
                Qtde: grupo.Count()
            )).ToArray();

        return new DadosResumoMinhasReceitas(
            ReceitaRecebida: receitaRecebida,
            ReceitaEstimada: receitaEstimada,
            ReceitaRecebidaMesAnterior: receitaRecebidaMesAnterior,
            PorProcedimento: receitaPorProcedimento
        );
    }
}