namespace AgendAi.Domain.Profissionais;
using AgendAi.Domain.Organizacoes;

public class Procedimento
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual string Nome { get; set; } = string.Empty;
    public virtual int DuracaoEstimadaMinutos { get; set; } = 30;
    public virtual decimal ValorBase { get; set; }
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual DateTimeOffset? UpdatedAt { get; set; }
    public virtual DateTimeOffset? DeletedAt { get; set; }
}
