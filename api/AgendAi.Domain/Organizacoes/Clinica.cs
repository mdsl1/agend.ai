namespace AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Organizacoes;


public class Clinica
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual string? Cnpj { get; set; }
    public virtual string Nome { get; set; } = string.Empty;
    public virtual string? Telefone { get; set; }
    public virtual string? Endereco { get; set; }
    public virtual string TipoClinica { get; set; } = "medica";
    public virtual string? WebhookCalendar { get; set; }
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual DateTimeOffset? UpdatedAt { get; set; }
    public virtual DateTimeOffset? DeletedAt { get; set; }
    public virtual IList<HorarioFuncionamento> HorariosFuncionamento { get; set; } = new List<HorarioFuncionamento>();
}
