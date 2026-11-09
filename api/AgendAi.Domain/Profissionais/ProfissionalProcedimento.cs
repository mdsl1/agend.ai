namespace AgendAi.Domain.Profissionais;
using AgendAi.Domain.Organizacoes;
using AgendAi.Domain.Common.Exceptions;
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

    public static ProfissionalProcedimento CriarVinculo(
        Profissional profissional,
        Procedimento procedimento,
        int duracaoMinutos,
        decimal valor
    )
    {
        ArgumentNullException.ThrowIfNull(profissional);
        ArgumentNullException.ThrowIfNull(procedimento);

        if (profissional.Clinica.Uuid != procedimento.Clinica.Uuid)
        {
            throw new RegraNegocioException(
                "dados_clinicas_diferentes",
                "Profissional e procedimento devem pertencer à mesma clínica."
            );
        }

        if (duracaoMinutos <= 0)
        {
            throw new RegraNegocioException(
                "duracao_negativa",
                "A duração de um procedimento deve ser maior que zero."
            );
        }

        if (valor < 0)
        {
            throw new RegraNegocioException(
                "valor_invalido",
                "O valor de um procedimento não pode ser negativo."
            );
        }

        return new ProfissionalProcedimento
        {
            Uuid = Guid.NewGuid(),
            Clinica = profissional.Clinica,
            Profissional = profissional,
            Procedimento = procedimento,
            Valor = valor,
            DuracaoMinutos = duracaoMinutos,
            CreatedAt = DateTime.UtcNow
        };
    }

    public virtual void AlterarVinculo(
        int duracaoMinutos,
        decimal valor,
        DateTime? deletedAt
    )
    {
        if (duracaoMinutos <= 0)
        {
            throw new RegraNegocioException(
                "duracao_negativa",
                "A duração de um procedimento deve ser maior que zero."
            );
        }

        if (valor < 0)
        {
            throw new RegraNegocioException(
                "valor_invalido",
                "O valor de um procedimento não pode ser negativo."
            );
        }

        DuracaoMinutos = duracaoMinutos;
        Valor = valor;
        DeletedAt = deletedAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
