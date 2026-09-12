namespace AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;

public class Cliente
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual string Nome { get; set; } = string.Empty;
    public virtual string? Cpf { get; set; }
    public virtual string? Email { get; set; }
    public virtual string? IdTelegram { get; set; }
    public virtual string Telefone { get; set; } = string.Empty;
    public virtual DateOnly? DataNascimento { get; set; }
    public virtual string? Genero { get; set; }
    public virtual string? ObservacoesAnamnese { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
}
