import { sessionService } from './session';

type ApiRequestOptions = {
    method?: 'GET' | 'POST' | 'PATCH' | 'DELETE';
    body?: unknown;
    authenticated?: boolean;
};

type ProblemDetails = {
    title?: string;
    detail?: string;
};

export class ApiError extends Error {
    constructor(
        message: string,
        public readonly status: number,
    ) {
        super(message);
        this.name = 'ApiError';
    }
}

export class ApiConfigurationError extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'ApiConfigurationError';
    }
}

function getApiBaseUrl(): string {
    const configuredUrl = process.env.EXPO_PUBLIC_API_URL?.trim();

    if (!configuredUrl || configuredUrl.includes('SEU_IP')) {
        throw new ApiConfigurationError(
            'Configure EXPO_PUBLIC_API_URL no arquivo .env.local antes de iniciar o aplicativo.',
        );
    }

    return configuredUrl.replace(/\/+$/, '');
}

async function readResponse<T>(response: Response): Promise<T> {
    const responseText = await response.text();

    if (!response.ok) {
        let message = `A API respondeu com o status ${response.status}.`;

        if (responseText) {
            try {
                const problem = JSON.parse(responseText) as ProblemDetails;
                message = problem.detail ?? problem.title ?? message;
            } catch {
                message = responseText;
            }
        }

        throw new ApiError(message, response.status);
    }

    if (!responseText) {
        return undefined as T;
    }

    return JSON.parse(responseText) as T;
}

/** Cliente HTTP compartilhado por todas as funcionalidades do aplicativo. */
export async function apiRequest<T>(
    path: string,
    options: ApiRequestOptions = {},
): Promise<T> {
    const apiBaseUrl = getApiBaseUrl();
    const headers: Record<string, string> = {
        Accept: 'application/json',
    };

    if (options.body !== undefined) {
        headers['Content-Type'] = 'application/json';
    }

    if (options.authenticated) {
        const token = await sessionService.getToken();

        if (!token) {
            throw new ApiError('Sua sessão terminou. Entre novamente.', 401);
        }

        headers.Authorization = `Bearer ${token}`;
    }

    let response: Response;

    try {
        response = await fetch(`${apiBaseUrl}${path}`, {
            method: options.method ?? 'GET',
            headers,
            ...(options.body !== undefined
                ? { body: JSON.stringify(options.body) }
                : {}),
        });
    } catch {
        throw new Error(
            'Não foi possível conectar à API. Confira se ela está rodando e se a URL está correta para este dispositivo.',
        );
    }

    return readResponse<T>(response);
}
