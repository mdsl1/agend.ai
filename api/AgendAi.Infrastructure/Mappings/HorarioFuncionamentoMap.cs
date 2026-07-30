namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Organizacoes;
using FluentNHibernate.Mapping;

public class HorarioFuncionamentoMap : ClassMap <HorarioFuncionamento>
{
    public HorarioFuncionamentoMap ()
    {
        Table("horario_funcionamento");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable().UniqueKey("uq_clinica_dia");
        Map(x => x.DiaSemana).Column("dia_semana").Not.Nullable()
            .Check("dia_semana BETWEEN 0 AND 6")
            .UniqueKey("uq_clinica_dia");
        Map(x => x.HoraInicio).Column("hora_inicio").CustomType("TimeAsTimeSpan").Not.Nullable();
        Map(x => x.HoraFim).Column("hora_fim").CustomType("TimeAsTimeSpan").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
    }
} 
