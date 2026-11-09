export type ProcedimentoCatalogo = {
    uuid: string;
    nome: string;
    duracaoEstimadaMinutos: number;
    valorBase: number;
};

export type ProcedimentoProfissional = {
    profissionalProcedimentoUuid: string;
    procedimentoUuid: string;
    nome: string;
    valorEfetivo: number;
    duracaoEfetivaMinutos: number;
};

export type ProcedimentoExibido = {
    uuid: string;
    profissionalProcedimentoUuid?: string;
    nome: string;
    ativo: boolean;
    valor: number;
    duracaoMinutos: number;
    valorBase: number;
    duracaoEstimadaMinutos: number;
};

export type RequisicaoVincularMeuProcedimento = {
    procedimentoUuid: string;
    duracaoMinutos?: number | null;
    valor?: number | null;
};

export type RespostaVincularMeuProcedimento = {
    profissionalProcedimentoUuid: string;
    criado: boolean;
    nome: string;
    duracaoMinutos: number;
    valor: number;
};

export type RequisicaoAlterarMeuProcedimento = {
    duracaoMinutos?: number | null;
    valor?: number | null;
    disponivelAgendamento?: boolean | null;
};

export type RespostaAlterarMeuProcedimento = {
    profissionalProcedimentoUuid: string;
    disponivelAgendamento: boolean;
    nome: string;
    duracaoMinutos: number;
    valor: number;
};

export type RespostaCatalogoProcedimentos = {
    procedimentos: ProcedimentoCatalogo[];
};

export type RespostaProcedimentosProfissional = {
    procedimentos: ProcedimentoProfissional[];
};
