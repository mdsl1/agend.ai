namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class DisponibilidadeReader : IDisponibilidadeReader
{
    private readonly ISession _session;
    public DisponibilidadeReader ( ISession session )
    {
        _session = session;
    }

    public async Task<DadosDisponibilidadeProfissional?> ObterAsync (
        Guid profissionalUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    )
    {
        var context = await _session
            .Query<ProfissionalProcedimento>()
            .Where(vinculo => 
                vinculo.Uuid == profissionalProcedimentoUuid
                && vinculo.Profissional.Uuid == profissionalUuid
                && vinculo.DeletedAt == null
                && vinculo.Procedimento.DeletedAt == null
                && vinculo.Clinica.DeletedAt == null
                && vinculo.Profissional.Usuario.DeletedAt == null
            )
            .Select(vinculo => new
            {
                ProfissionalUuid = vinculo.Profissional.Uuid,
                ProfissionalProcedimentoUuid = vinculo.Uuid,
                ClinicaId = vinculo.Clinica.Id,
                ClinicaUuid = vinculo.Clinica.Uuid,
                DuracaoMinutos = vinculo.DuracaoMinutos,
                IdAgendaExterna = vinculo.Profissional.IdGoogleCalendar,
                UrlWebhookAgenda = vinculo.Clinica.WebhookCalendar
            })
            .SingleOrDefaultAsync(cancellationToken);

        if(context is null)
        {
            return null;
        }

        var janelasAtendimento = await _session
            .Query<HorarioFuncionamento>()
            .Where(horario => horario.Clinica.Id == context.ClinicaId)
            .OrderBy(horario => horario.DiaSemana)
            .Select(horario => new DadosJanelaAtendimento (
                horario.DiaSemana,
                horario.HoraInicio,
                horario.HoraFim
            ))
            .ToListAsync(cancellationToken);
            
        return new DadosDisponibilidadeProfissional(
            ProfissionalUuid: context.ProfissionalUuid,
            ProfissionalProcedimentoUuid: context.ProfissionalProcedimentoUuid,
            ClinicaUuid: context.ClinicaUuid,
            DuracaoMinutos: context.DuracaoMinutos,
            IdAgendaExterna: context.IdAgendaExterna,
            UrlWebhookAgenda: context.UrlWebhookAgenda,
            JanelasAtendimento: janelasAtendimento
        );
    }
}