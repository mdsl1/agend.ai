export type ResumoInicio = {
    dataReferencia: string;
    resumoReceitas: {
        periodo: { inicio: string; fim: string };
        valorTotal: number;
        percentualVsMesAnterior: number | null;
    };
    resumoAgenda: {
        qtdeHoje: number;
        qtdeConfirmados: number;
        qtdeRestante: number;
        qtdeCancelados: number;
        proximoAgendamento: {
            agendamentoUuid: string;
            inicio: string;
            fim: string;
            nomeCliente: string;
            nomeProcedimento: string;
            status: string;
        } | null;
    };
};
