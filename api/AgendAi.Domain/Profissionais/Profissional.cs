using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Usuarios;

namespace AgendAi.Domain.Profissionais;

public class Profissional
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Especialidade? Especialidade { get; set; }
    public virtual string? RegistroProfissional { get; set; }
    public virtual string? IdGoogleCalendar { get; set; }
    public virtual string? Prefixo { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
}
