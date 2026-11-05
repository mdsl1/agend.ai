import { apiRequest } from '../../services/api';
import type { RespostaCatalogoProcedimentos, RespostaProcedimentosProfissional } from './typeProcedimentos';

export async function obterMeusProcedimentos(): Promise<RespostaProcedimentosProfissional> {
    return apiRequest<RespostaProcedimentosProfissional>('/api/me/procedimentos', { authenticated: true });
}

export async function obterCatalogoProcedimentos(): Promise<RespostaCatalogoProcedimentos> {
    return apiRequest<RespostaCatalogoProcedimentos>('/api/procedimentos', { authenticated: true });
}
