namespace AgendAi.Domain.Profissionais;
using AgendAi.Domain.Organizacoes;

public class ProfissionalProcedimento
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual Profissional Profissional { get; set; } = null!;
    public virtual Procedimento Procedimento { get; set; } = null!;
    public virtual decimal Valor { get; set; }
    public virtual int DuracaoMinutos { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
}
