import { apiRequest } from '../../services/api';
import type { LoginRequest, LoginResponse, MeuPerfil, ResolverClinicaResponse } from './authTypes';

export function resolveClinic(code: string): Promise<ResolverClinicaResponse> {
    const query = encodeURIComponent(code.trim());
    return apiRequest<ResolverClinicaResponse>(
        `/api/clinicas/resolver?codigoIdentificador=${query}`,
    );
}

export function login(
    clinicUuid: string,
    email: string,
    password: string,
): Promise<LoginResponse> {
    const body: LoginRequest = {
        clinicaUuid: clinicUuid,
        email: email.trim(),
        senha: password,
    };

    return apiRequest<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body,
    });
}

/** Busca perfil e permissões no servidor; o app não decodifica o JWT. */
export function getMyProfile(): Promise<MeuPerfil> {
    return apiRequest<MeuPerfil>('/api/me', { authenticated: true });
}
