namespace AgendAi.Domain.Usuarios;
using AgendAi.Domain.Organizacoes;

public class Usuario
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual string Nome { get; set; } = string.Empty;
    public virtual string Email { get; set; } = string.Empty;
    public virtual string SenhaHash { get; set; } = string.Empty;
    public virtual string Cargo { get; set; } = string.Empty;
    public virtual bool IsAdmin { get; set; }
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual DateTimeOffset? UpdatedAt { get; set; }
    public virtual DateTimeOffset? DeletedAt { get; set; }
}
