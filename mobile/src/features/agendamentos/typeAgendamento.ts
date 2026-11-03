import type { StatusAgendamento } from '../agenda/typeAgenda';

export type DetalhesAgendamento = {
    uuid: string;
    cliente: {
        uuid: string;
        nome: string;
        telefone: string;
    };
    profissional: {
        uuid: string;
        nomeExibicao: string;
    };
    procedimento: {
        uuid: string;
        nome: string;
        duracaoMinutos: number;
    };
    inicio: string;
    fim: string;
    motivoContato: string | null;
    anotacoesProfissional: string | null;
    valorTotal: number;
    status: StatusAgendamento;
    statusPagamento: string;
};

export type AlteracaoStatusAgendamento = {
    agendamentoUuid: string;
    statusAnterior: StatusAgendamento;
    novoStatus: StatusAgendamento;
    alteradoEm: string;
};
