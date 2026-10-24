export type ResumoReceitas = {
    periodo: {
        inicio: string;
        fim: string;
    };
    receitaRecebida: number;
    receitaEstimada: number;
    percentualVsMesAnterior: number | null;
    porProcedimento: {
        procedimentoUuid: string;
        nome: string;
        qtde: number;
        valorTotal: number;
    }[];
};
