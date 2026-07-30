namespace AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Organizacoes;

public class HorarioFuncionamento
{
    public virtual long Id { get; set; }
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual short DiaSemana { get; set; }
    public virtual TimeSpan HoraInicio { get; set; }
    public virtual TimeSpan HoraFim { get; set; }
    public virtual DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
