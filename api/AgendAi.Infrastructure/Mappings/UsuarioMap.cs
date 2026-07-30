namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Usuarios;
using FluentNHibernate.Mapping;

public class UsuarioMap : ClassMap<Usuario>
{
    public UsuarioMap()
    {
        Table("usuarios");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable()
            .UniqueKey("uq_usuario_email_clinica");
        Map(x => x.Nome).Column("nome").Length(150).Not.Nullable();
        Map(x => x.Email).Column("email").Length(150).Not.Nullable()
            .UniqueKey("uq_usuario_email_clinica");
        Map(x => x.SenhaHash).Column("senha_hash").Length(255).Not.Nullable();
        Map(x => x.Cargo).Column("cargo").Length(50).Not.Nullable();
        Map(x => x.IsAdmin).Column("is_admin").Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}
