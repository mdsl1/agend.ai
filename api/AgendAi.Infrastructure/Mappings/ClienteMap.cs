namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Clientes;
using FluentNHibernate.Mapping;


public class ClienteMap : ClassMap<Cliente>
{
    public ClienteMap()
    {
        Table("clientes");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        References(x => x.Clinica).Column("id_clinica").Not.Nullable();
        Map(x => x.Nome).Column("nome").Length(150).Not.Nullable();
        Map(x => x.Cpf).Column("cpf").Length(14);
        Map(x => x.Email).Column("email").Length(150);
        Map(x => x.Telefone).Column("telefone").Length(20).Not.Nullable();
        Map(x => x.DataNascimento).Column("data_nascimento");
        Map(x => x.Genero).Column("genero").Length(20);
        Map(x => x.ObservacoesAnamnese).Column("observacoes_anamnese").CustomSqlType("text");
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");
    }
}
