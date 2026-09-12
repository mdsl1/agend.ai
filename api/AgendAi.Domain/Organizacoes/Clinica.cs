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
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual IList<HorarioFuncionamento> HorariosFuncionamento { get; set; } = new List<HorarioFuncionamento>();
}
