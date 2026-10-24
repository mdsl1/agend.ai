import { apiRequest } from '../../services/api';
import type { RespostaAgenda } from './typeAgenda';

export function obterEventosAgenda(
    profissionalUuid: string,
    inicio: string,
    fim: string,
): Promise<RespostaAgenda> {
    const parametros = new URLSearchParams({ inicio, fim });
    return apiRequest<RespostaAgenda>(
        `/api/agenda/${encodeURIComponent(profissionalUuid)}?${parametros.toString()}`,
        { authenticated: true },
    );
}
