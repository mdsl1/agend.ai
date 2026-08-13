namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Agendamentos;
using FluentNHibernate.Mapping;


public class AgendamentoMap : ClassMap<Agendamento>
{
    public AgendamentoMap()
    {
        Table("agendamentos");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable();
        References(x => x.Cliente).Column("id_cliente").Not.Nullable();
        References(x => x.Profissional).Column("id_profissional").Not.Nullable();
        References(x => x.Especialidade).Column("id_especialidade").Nullable();
        References(x => x.Procedimento).Column("id_procedimento").Nullable();
        Map(x => x.IdEventGoogleCalendar).Column("id_event_google_calendar").CustomSqlType("text");
        Map(x => x.TimeDateInicio).Column("timedate_inicio").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.TimeDateFim).Column("timedate_fim").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.MotivoContato).Column("motivo_contato").CustomSqlType("text");
        Map(x => x.AnotacoesProfissional).Column("anotacoes_profissional").CustomSqlType("text");
        Map(x => x.ValorTotal).Column("valor_total").Precision(10).Scale(2).Not.Nullable();
        Map(x => x.StatusPagamento).Column("status_pagamento").Length(20).Not.Nullable();
        Map(x => x.Status).Column("status").Length(20).Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}
