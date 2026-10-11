namespace AgendAi.Application.Receitas.Ports;

using AgendAi.Application.Receitas.Models;

public interface IResumoMinhasReceitasReader
{
    Task<DadosResumoMinhasReceitas> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        DateTimeOffset inicioMesAnterior,
        DateTimeOffset inicioMes,
        DateTimeOffset inicioProximoMes,
        CancellationToken cancellationToken
    );
}