namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Profissionais;
using AgendAi.Domain.Agendamentos;
using NHibernate;
using NHibernate.Linq;

public sealed class CriacaoAgendamentoReader : ICriacaoAgendamentoReader
{
    private readonly ISession _session;

    public CriacaoAgendamentoReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosCriacaoAgendamento?> ObterAsync(
        Guid clienteUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    )
    {
        var profissionalProcedimento = await _session
            .Query<ProfissionalProcedimento>()
            .Where(vinculo => 
                vinculo.Uuid == profissionalProcedimentoUuid
                && vinculo.DeletedAt == null
                && vinculo.Procedimento.DeletedAt == null
                && vinculo.Clinica.DeletedAt == null
                && vinculo.Profissional.Usuario.DeletedAt == null
            ).SingleOrDefaultAsync(cancellationToken);

        if(profissionalProcedimento is null)
        {
            return null;
        }

        var cliente = await _session
            .Query<Cliente>()
            .Where(cliente =>
                cliente.Uuid == clienteUuid
                && cliente.DeletedAt == null
                && cliente.Clinica.DeletedAt == null
                && cliente.Clinica.Id == profissionalProcedimento.Clinica.Id
            ).SingleOrDefaultAsync(cancellationToken);
        
        if(cliente is null)
        {
            return null;
        }

        var janelasAtendimento = await _session
            .Query<HorarioFuncionamento>()
            .Where(horario => horario.Clinica.Id == profissionalProcedimento.Clinica.Id)
            .OrderBy(horario => horario.DiaSemana)
            .Select(horario => new DadosJanelaAtendimento(
                horario.DiaSemana,
                horario.HoraInicio,
                horario.HoraFim
            )).ToListAsync(cancellationToken);

        return new DadosCriacaoAgendamento(
            Cliente: cliente,
            ProfissionalProcedimento: profissionalProcedimento,
            JanelasAtendimento: janelasAtendimento
        );
    }

    public async Task<Agendamento?> ObterPorChaveIdempotenciaAsync(
        Guid clinicaUuid,
        string chaveIdempotencia,
        CancellationToken cancellationToken
    )
    {
        return await _session
            .Query<Agendamento>()
            .Where(agendamento  =>
                agendamento.Clinica.Uuid == clinicaUuid
                && agendamento.ChaveIdempotencia == chaveIdempotencia.Trim()
            ).SingleOrDefaultAsync(cancellationToken);
    }
}