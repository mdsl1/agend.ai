using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Profissionais;

namespace AgendAi.Domain.Agendamentos;

public class Agendamento
{
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Profissional Profissional { get; set; } = null!;
    public virtual Especialidade? Especialidade { get; set; }
    public virtual Procedimento? Procedimento { get; set; }
    public virtual DateTimeOffset TimeDateInicio { get; set; }
    public virtual DateTimeOffset TimeDateFim { get; set; }
    public virtual string? MotivoContato { get; set; }
    public virtual string? AnotacoesProfissional { get; set; }
    public virtual decimal ValorTotal { get; set; }
    public virtual string StatusPagamento { get; set; } = "pendente";
    public virtual string Status { get; set; } = "agendado";
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual DateTimeOffset? UpdatedAt { get; set; }
    public virtual DateTimeOffset? DeletedAt { get; set; }
}
