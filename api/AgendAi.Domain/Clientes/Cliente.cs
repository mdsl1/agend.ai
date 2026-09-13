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

    
    public static Cliente Criar(
        Clinica clinica,
        string nome,
        string telefone,
        string? telegramUserId
    )
    {
        ArgumentNullException.ThrowIfNull(clinica);
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(telefone);

        return new Cliente 
        {
            Uuid = Guid.NewGuid(),
            Clinica = clinica,
            Nome = nome.Trim(),
            Telefone =telefone.Trim(),
            IdTelegram = string.IsNullOrWhiteSpace(telegramUserId)
                ? null
                : telegramUserId.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public virtual void VincularTelegram(string telegramUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(telegramUserId);

        var id = telegramUserId.Trim();

        if (!string.IsNullOrWhiteSpace(IdTelegram) && !string.Equals(IdTelegram, id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("O cliente já possui outro usuário do telegram vinculado.");
        }

        if (string.Equals(IdTelegram, id, StringComparison.Ordinal))
        {
            return;
        }

        IdTelegram = id;
        UpdatedAt = DateTime.UtcNow;
    }
}