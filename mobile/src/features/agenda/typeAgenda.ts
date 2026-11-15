export type StatusAgendamento =
    | 'pendente_integracao'
    | 'agendado'
    | 'confirmado'
    | 'reagendado'
    | 'falha_integracao'
    | 'concluido'
    | 'cancelado'
    | 'faltou';

export type FrequenciaIndisponibilidade = 'Daily' | 'Weekly' | 'Monthly' | 'Yearly';

export type EventoAgenda = {
    id: string;
    tipo: 'agendamento' | 'indisponibilidade';
    agendamentoUuid: string | null;
    statusAgendamento?: StatusAgendamento | null;
    titulo: string;
    inicio: string;
    fim: string;
    nomeCliente: string | null;
    nomeProcedimento: string | null;
    profissionalUuid: string;
};

export type RespostaAgenda = {
    eventos: EventoAgenda[];
};

export type CriarMinhaIndisponibilidadeRequest = {
    inicio: string;
    fim: string;
    motivo: string | null;
    seRepete: boolean;
    frequencia: FrequenciaIndisponibilidade | null;
    repetirAte: string | null;
};

export type CriarMinhaIndisponibilidadeResponse = {
    id: string;
    tipo: string;
    titulo: string;
    inicio: string;
    fim: string;
    seRepete: boolean;
    frequencia: FrequenciaIndisponibilidade | null;
    repetirAte: string | null;
};
