namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Profissionais;
using FluentNHibernate.Mapping;

public class ProfissionalProcedimentoMap : ClassMap<ProfissionalProcedimento>
{
    public ProfissionalProcedimentoMap() {
        Table("profissional_procedimentos");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable();
        References(x => x.Profissional).Column("id_profissional").Not.Nullable();
        References(x => x.Procedimento).Column("id_procedimento").Not.Nullable();
        Map(x => x.Valor).Column("valor").Precision(10).Scale(2).Not.Nullable();
        Map(x => x.DuracaoMinutos).Column("duracao_minutos").Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}