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
        References(x => x.Usuario).Column("id_usuario").Not.Nullable().Unique();
        References(x => x.Especialidade).Column("id_especialidade").Nullable();
        Map(x => x.RegistroProfissional).Column("registro_profissional").Length(30);
    }
}
