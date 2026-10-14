export type LoginRequest = {
    clinicaUuid: string;
    email: string;
    senha: string;
};

export type LoginResponse = {
    accessToken: string;
    expiraEm: string;
    usuario: {
        uuid: string;
        nome: string;
        cargo: string;
        isAdmin: boolean;
        profissionalUuid: string | null;
        clinicaUuid: string;
    };
};

export type MeuPerfil = {
    usuarioUuid: string;
    profissionalUuid: string | null;
    nome: string;
    prefixo: string | null;
    email: string;
    cargo: string;
    isAdmin: boolean;
    registroProfissional: string | null;
    especialidade: string | null;
    clinica: {
        uuid: string;
        nome: string;
        tipoClinica: string;
    };
    permissoes: string[];
};
