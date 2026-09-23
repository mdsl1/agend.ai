namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Profissionais;
using FluentNHibernate.Mapping;

public class ProfissionalMap : ClassMap<Profissional>
{
    public ProfissionalMap()
    {
        Table("profissionais");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable();
        References(x => x.Usuario).Column("id_usuario").Not.Nullable().Unique();
        References(x => x.Especialidade).Column("id_especialidade").Nullable();
        Map(x => x.RegistroProfissional).Column("registro_profissional").Length(30);
        Map(x => x.IdGoogleCalendar).Column("id_google_calendar").CustomSqlType("text");
        Map(x => x.Prefixo).Column("prefixo").Length(6);
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}
