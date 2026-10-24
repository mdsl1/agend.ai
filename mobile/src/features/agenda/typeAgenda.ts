export type StatusAgendamento =
    | 'pendente_integracao'
    | 'agendado'
    | 'reagendado'
    | 'falha_integracao'
    | 'concluido'
    | 'cancelado'
    | 'faltou';

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
