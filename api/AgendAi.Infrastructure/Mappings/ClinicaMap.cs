namespace AgendAi.Infrastructure.Mappings;

using AgendAi.Domain.Organizacoes;
using FluentNHibernate.Mapping;

public class ClinicaMap : ClassMap <Clinica>
{
    public ClinicaMap()
    {
        Table("clinicas");
        Id(x => x.Id).Column("id").GeneratedBy.Native();
        Map(x => x.Uuid).Column("uuid").Not.Nullable().Unique();
        Map(x => x.Cnpj).Column("cnpj").Length(18).Unique();
        Map(x => x.Nome).Column("nome").Length(150).Not.Nullable();
        Map(x => x.Telefone).Column("telefone").Length(20);
        Map(x => x.Endereco).Column("endereco").CustomSqlType("text");
        Map(x => x.TipoClinica).Column("tipo_clinica").Length(50).Not.Nullable();
        Map(x => x.WebhookCalendar).Column("webhook_calendar").CustomSqlType("text");
        Map(x => x.CreatedAt).Column("created_at").CustomSqlType("timestamptz").Not.Nullable();
        Map(x => x.UpdatedAt).Column("updated_at").CustomSqlType("timestamptz");
        Map(x => x.DeletedAt).Column("deleted_at").CustomSqlType("timestamptz");

        HasMany(x => x.HorariosFuncionamento).KeyColumn("id_clinica").Inverse().Cascade.All();
    }
}
