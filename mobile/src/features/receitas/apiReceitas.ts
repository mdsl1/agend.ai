import { apiRequest } from '../../services/api';
import type { ResumoReceitas } from './typeReceitas';

export function obterResumoReceitas(mes: number, ano: number): Promise<ResumoReceitas> {
    const parametros = new URLSearchParams({ mes: String(mes), ano: String(ano) });
    return apiRequest<ResumoReceitas>(`/api/me/receitas/resumo?${parametros.toString()}`, {
        authenticated: true,
    });
}
