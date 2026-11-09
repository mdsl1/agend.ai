import { apiRequest } from '../../services/api';
import type {
    RequisicaoAlterarMeuProcedimento,
    RequisicaoVincularMeuProcedimento,
    RespostaAlterarMeuProcedimento,
    RespostaCatalogoProcedimentos,
    RespostaProcedimentosProfissional,
    RespostaVincularMeuProcedimento,
} from './typeProcedimentos';

export async function obterMeusProcedimentos(): Promise<RespostaProcedimentosProfissional> {
    return apiRequest<RespostaProcedimentosProfissional>('/api/me/procedimentos', { authenticated: true });
}

export async function obterCatalogoProcedimentos(): Promise<RespostaCatalogoProcedimentos> {
    return apiRequest<RespostaCatalogoProcedimentos>('/api/procedimentos', { authenticated: true });
}

export async function vincularMeuProcedimento(
    dados: RequisicaoVincularMeuProcedimento,
): Promise<RespostaVincularMeuProcedimento> {
    return apiRequest<RespostaVincularMeuProcedimento>('/api/me/procedimentos', {
        method: 'POST',
        body: dados,
        authenticated: true,
    });
}

export async function alterarMeuProcedimento(
    profissionalProcedimentoUuid: string,
    dados: RequisicaoAlterarMeuProcedimento,
): Promise<RespostaAlterarMeuProcedimento> {
    return apiRequest<RespostaAlterarMeuProcedimento>(
        `/api/me/procedimentos/${profissionalProcedimentoUuid}`,
        {
            method: 'PATCH',
            body: dados,
            authenticated: true,
        },
    );
}
