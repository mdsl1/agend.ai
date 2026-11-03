namespace AgendAi.Domain.Agendamentos;

using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Profissionais;
using AgendAi.Domain.Common.Exceptions;

public class Agendamento
{
    public const int DiasToleranciaParaCancelamento = -1;
    public const string StatusPendenteIntegracao = "pendente_integracao";
    public const string StatusAgendado = "agendado";
    public const string StatusConcluido = "concluido";
    public const string StatusFalhaIntegracao = "falha_integracao";
    public const string StatusReagendado = "reagendado";
    public const string StatusCancelado = "cancelado";
    public const string StatusFaltou = "faltou";
    public const string StatusPagamentoPendente = "pendente";
    public const string StatusPagamentoPago = "pago";
    public const string StatusPagamentoIsento = "isento";
    public const string StatusPagamentoEstornado = "estornado";
    public virtual long Id { get; set; }
    public virtual Guid Uuid { get; set; } = Guid.NewGuid();
    public virtual string ChaveIdempotencia { get; set; } = string.Empty;
    public virtual Clinica Clinica { get; set; } = null!;
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Profissional Profissional { get; set; } = null!;
    public virtual Especialidade Especialidade { get; set; } = null!;
    public virtual Procedimento Procedimento { get; set; } = null!;
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
            throw new RegraNegocioException(
                "dados_clinicas_diferentes",
                "Cliente e profissional devem pertencer à mesma clínica."
            );
        }

        if (fim <= inicio)
        {
            throw new RegraNegocioException(
                "periodos_invalidos",
                "O fim do agendamento deve ser posterior ao início."
            );
        }

        return new Agendamento
        {
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
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Somente um agendamento pendente pode ser confirmado."
            );
        }

        IdEventGoogleCalendar = idEventGoogleCalendar.Trim();
        Status = StatusAgendado;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void RegistrarFalhaIntegracao()
    {
        if (Status != StatusPendenteIntegracao)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Somente um agendamento pendente pode registrar uma falha de integração."
            );
        }

        Status = StatusFalhaIntegracao;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void MarcarComoFaltou(DateTime datetimeAlteracao)
    {
        if (datetimeAlteracao.Date < TimeDateInicio.Date)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Não é possível marcar uma falta antes de chegar o dia agendado."
            );
        }

        Status = StatusFaltou;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void MarcarComoCancelado(DateTime datetimeAlteracao)
    {
        var dataLimite = TimeDateInicio.Date.AddDays(DiasToleranciaParaCancelamento);

        if (Status == StatusConcluido)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Não é possível cancelar um agendamento concluído."
            );
        }

        if (datetimeAlteracao.Date >= dataLimite)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                $"O prazo de cancelamento é de no máximo d{DiasToleranciaParaCancelamento}. Após esse período, deverá ser solicitado reagendamento."
            );
        }

        Status = StatusCancelado;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void MarcarComoConcluido(DateTime datetimeAlteracao)
    {
        if (datetimeAlteracao.Date <= TimeDateInicio.Date)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Não é possível concluir um agendamento antes do dia agendado."
            );
        }

        if (Status != StatusAgendado)
        {
            throw new RegraNegocioException(
                "alteracao_nao_permitida",
                "Somente um agendamento com status 'agendado' pode ser concluído."
            );
        }

        Status = StatusConcluido;
        UpdatedAt = DateTime.UtcNow;
    }
}
