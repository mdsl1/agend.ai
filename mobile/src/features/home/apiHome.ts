import { apiRequest } from '../../services/api';
import type { ResumoInicio } from './homeTypes';

export function obterResumoInicio(): Promise<ResumoInicio> {
    return apiRequest<ResumoInicio>('/api/me/overview', { authenticated: true });
}
