namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Profissionais;
using FluentNHibernate.Mapping;

public class EspecialidadeMap : ClassMap<Especialidade>
{
    public EspecialidadeMap()
    {
        Table("especialidades");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable();
        Map(x => x.Nome).Column("nome").Length(100).Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}
