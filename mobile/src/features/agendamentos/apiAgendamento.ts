import { apiRequest } from '../../services/api';
import type { AlteracaoStatusAgendamento, DetalhesAgendamento } from './typeAgendamento';

export function obterDetalhesAgendamento(agendamentoUuid: string): Promise<DetalhesAgendamento> {
    return apiRequest<DetalhesAgendamento>(
        `/api/agendamentos/${encodeURIComponent(agendamentoUuid)}`,
        { authenticated: true },
    );
}

export function alterarStatusAgendamento(
    agendamentoUuid: string,
    status: 'concluido' | 'cancelado' | 'faltou',
): Promise<AlteracaoStatusAgendamento> {
    return apiRequest<AlteracaoStatusAgendamento>(
        `/api/agendamentos/${encodeURIComponent(agendamentoUuid)}/status`,
        { method: 'PATCH', authenticated: true, body: { status } },
    );
}
