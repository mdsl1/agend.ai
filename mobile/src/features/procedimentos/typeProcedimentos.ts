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
    nome: string;
    ativo: boolean;
    valor: number;
    duracaoMinutos: number;
};

export type RespostaCatalogoProcedimentos = {
    procedimentos: ProcedimentoCatalogo[];
};

export type RespostaProcedimentosProfissional = {
    procedimentos: ProcedimentoProfissional[];
};
