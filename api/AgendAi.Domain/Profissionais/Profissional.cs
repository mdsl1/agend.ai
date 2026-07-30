using AgendAi.Domain.Usuarios;

namespace AgendAi.Domain.Profissionais;

public class Profissional
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Especialidade? Especialidade { get; set; }
    public virtual string? RegistroProfissional { get; set; }
}