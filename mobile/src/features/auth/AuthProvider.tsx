import {
    createContext,
    useCallback,
    useContext,
    useEffect,
    useMemo,
    useState,
    type PropsWithChildren,
} from 'react';
import { ApiError } from '../../services/api';
import { sessionService } from '../../services/session';
import { getMyProfile, login } from './authApi';
import type { MeuPerfil } from './authTypes';

export type AuthState =
    | { status: 'loading' }
    | { status: 'signedOut' }
    | { status: 'signedIn'; profile: MeuPerfil }
    | { status: 'error'; message: string };

type AuthContextValue = {
    state: AuthState;
    saveClinic: (clinicUuid: string) => Promise<void>;
    changeClinic: () => Promise<void>;
    signIn: (email: string, password: string) => Promise<void>;
    signOut: () => Promise<void>;
    retryRestore: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

function getErrorMessage(error: unknown): string {
    if (error instanceof Error) {
        return error.message;
    }

    return 'Ocorreu um erro inesperado. Tente novamente.';
}

export function AuthProvider({ children }: PropsWithChildren) {
    const [state, setState] = useState<AuthState>({ status: 'loading' });

    const retryRestore = useCallback(async () => {
        setState({ status: 'loading' });

        try {
            const token = await sessionService.getToken();

            if (!token) {
                setState({ status: 'signedOut' });
                return;
            }

            const clinicUuid = await sessionService.getClinicUuid();
            if (!clinicUuid) {
                await sessionService.clearToken();
                setState({ status: 'signedOut' });
                return;
            }

            const profile = await getMyProfile();
            setState({ status: 'signedIn', profile });
        } catch (error) {
            if (error instanceof ApiError && error.status === 401) {
                await sessionService.clearToken();
                setState({ status: 'signedOut' });
                return;
            }

            setState({ status: 'error', message: getErrorMessage(error) });
        }
    }, []);

    useEffect(() => {
        void retryRestore();
    }, [retryRestore]);

    const saveClinic = useCallback(async (clinicUuid: string) => {
        await sessionService.clearToken();
        await sessionService.saveClinicUuid(clinicUuid);
        setState({ status: 'signedOut' });
    }, []);

    const changeClinic = useCallback(async () => {
        await sessionService.clearClinic();
        setState({ status: 'signedOut' });
    }, []);

    const signIn = useCallback(async (email: string, password: string) => {
        let tokenWasSaved = false;
        try {
            const clinicUuid = await sessionService.getClinicUuid();
            if (!clinicUuid) {
                throw new Error('Informe o UUID da clínica antes de entrar.');
            }

            const loginResponse = await login(clinicUuid, email, password);

            if (!loginResponse.accessToken) {
                throw new Error('A API não retornou um token de acesso.');
            }

            await sessionService.saveToken(loginResponse.accessToken);
            tokenWasSaved = true;
            const profile = await getMyProfile();
            setState({ status: 'signedIn', profile });
        } catch (error) {
            if (
                tokenWasSaved ||
                (error instanceof ApiError && error.status === 401)
            ) {
                await sessionService.clearToken();
            }

            setState({ status: 'signedOut' });
            throw error;
        }
    }, []);

    const signOut = useCallback(async () => {
        await sessionService.clearToken();
        setState({ status: 'signedOut' });
    }, []);

    const value = useMemo(
        () => ({ state, saveClinic, changeClinic, signIn, signOut, retryRestore }),
        [state, saveClinic, changeClinic, signIn, signOut, retryRestore],
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error('useAuth deve ser usado dentro de AuthProvider.');
    }

    return context;
}
