using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Profissionais;

namespace AgendAi.Domain.Agendamentos;

public class Agendamento
{
    public const string StatusPendenteIntegracao = "pendente_integracao";
    public const string StatusAgendado = "agendado";
    public const string StatusFalhaIntegracao = "falha_integracao";
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual string ChaveIdempotencia { get; set; } = string.Empty;
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Profissional Profissional { get; set; } = null!;
    public virtual Especialidade? Especialidade { get; set; }
    public virtual Procedimento? Procedimento { get; set; }
    public virtual string? IdEventGoogleCalendar { get; set; }
    public virtual DateTime TimeDateInicio { get; set; }
    public virtual DateTime TimeDateFim { get; set; }
    public virtual string? MotivoContato { get; set; }
    public virtual string? AnotacoesProfissional { get; set; }
    public virtual decimal ValorTotal { get; set; }
    public virtual string StatusPagamento { get; set; } = "pendente";
    public virtual string Status { get; set; } = StatusPendenteIntegracao;
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? DeletedAt { get; set; }

    public static Agendamento CriarPendente(
        string chaveIdempotencia,
        Cliente cliente,
        ProfissionalProcedimento profissionalProcedimento,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        string? motivoContato
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chaveIdempotencia);
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentNullException.ThrowIfNull(profissionalProcedimento);

        if (cliente.Clinica.Uuid != profissionalProcedimento.Clinica.Uuid)
        {
            throw new ArgumentException("Cliente e profissional devem pertencer à mesma clínica.");
        }

        if (fim <= inicio)
        {
            throw new ArgumentException("O fim do agendamento deve ser posterior ao início.");
        }

        return new Agendamento{
            Uuid = Guid.NewGuid(),
            ChaveIdempotencia = chaveIdempotencia.Trim(),
            Clinica = profissionalProcedimento.Clinica,
            Cliente = cliente,
            Profissional = profissionalProcedimento.Profissional,
            Especialidade = profissionalProcedimento.Profissional.Especialidade,
            Procedimento = profissionalProcedimento.Procedimento,
            TimeDateInicio = inicio.UtcDateTime,
            TimeDateFim = fim.UtcDateTime,
            MotivoContato = string.IsNullOrWhiteSpace(motivoContato) ? null : motivoContato.Trim(),
            ValorTotal = profissionalProcedimento.Valor,
            StatusPagamento = "pendente",
            Status = StatusPendenteIntegracao,
            CreatedAt = DateTime.UtcNow
        };
    }

    public virtual void ConfirmarIntegracao(string idEventGoogleCalendar)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idEventGoogleCalendar);

        if (Status != StatusPendenteIntegracao)
        {
            throw new InvalidOperationException("Somente um agendamento pendente pode ser confirmado.");
        }

        IdEventGoogleCalendar = idEventGoogleCalendar.Trim();
        Status = StatusAgendado;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void RegistrarFalhaIntegracao()
    {
        if (Status != StatusPendenteIntegracao)
        {
            throw new InvalidOperationException("Somente um agendamento pendente pode registrar uma falha de integração.");
        }

        Status = StatusFalhaIntegracao;
        UpdatedAt = DateTime.UtcNow;
    }
}
