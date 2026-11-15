import { apiRequest } from '../../services/api';
import type {
    CriarMinhaIndisponibilidadeRequest,
    CriarMinhaIndisponibilidadeResponse,
    RespostaAgenda,
} from './typeAgenda';

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

export function criarMinhaIndisponibilidade(
    dados: CriarMinhaIndisponibilidadeRequest,
): Promise<CriarMinhaIndisponibilidadeResponse> {
    return apiRequest<CriarMinhaIndisponibilidadeResponse>(
        '/api/me/indisponibilidade',
        {
            method: 'POST',
            body: dados,
            authenticated: true,
        },
    );
}

export function excluirMinhaIndisponibilidade(
    eventoId: string,
): Promise<void> {
    return apiRequest<void>(
        `/api/me/indisponibilidade/${encodeURIComponent(eventoId)}`,
        {
            method: 'DELETE',
            authenticated: true,
        },
    );
}
