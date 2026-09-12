namespace AgendAi.Domain.Profissionais;
using AgendAi.Domain.Organizacoes;

public class Especialidade
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual string Nome { get; set; } = string.Empty;
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
}
