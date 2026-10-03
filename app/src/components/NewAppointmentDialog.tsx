import { useEffect, useMemo, useRef, useState } from "react";
import * as Dialog from "@radix-ui/react-dialog";
import {
  AlertCircle,
  CalendarClock,
  LoaderCircle,
  Search,
  X,
} from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "../features/api/apiClient";
import {
  criarAgendamento,
  listarProcedimentosProfissional,
  type ProcedimentoProfissionalApi,
} from "../features/agenda/agendaApi";
import {
  listarClientes,
  type ClienteApi,
} from "../features/clientes/clientesApi";

export type AppointmentSlot = { inicio: Date };
type ClientFilter = "nome" | "telefone";
type NewAppointmentDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: () => void;
  slot: AppointmentSlot | null;
  professionalName: string;
  professionalUuid: string;
};

type PendingAppointmentIntent = {
  idempotencyKey: string;
  fingerprint: string;
};

const PENDING_APPOINTMENT_STORAGE_KEY = "agendai:pending-appointment";

function getIdempotencyKey(fingerprint: string) {
  let pendingIntent: PendingAppointmentIntent | null = null;

  try {
    const storedIntent = sessionStorage.getItem(
      PENDING_APPOINTMENT_STORAGE_KEY,
    );

    if (storedIntent) {
      const parsedIntent: unknown = JSON.parse(storedIntent);
      if (
        typeof parsedIntent === "object" &&
        parsedIntent !== null &&
        "idempotencyKey" in parsedIntent &&
        typeof parsedIntent.idempotencyKey === "string" &&
        "fingerprint" in parsedIntent &&
        typeof parsedIntent.fingerprint === "string"
      ) {
        pendingIntent = {
          idempotencyKey: parsedIntent.idempotencyKey,
          fingerprint: parsedIntent.fingerprint,
        };
      }
    }

    if (pendingIntent?.fingerprint === fingerprint) {
      return pendingIntent.idempotencyKey;
    }

    const idempotencyKey = crypto.randomUUID();
    sessionStorage.setItem(
      PENDING_APPOINTMENT_STORAGE_KEY,
      JSON.stringify({ idempotencyKey, fingerprint }),
    );
    return idempotencyKey;
  } catch {
    throw new Error(
      "Não foi possível preparar a chave segura para esta tentativa. Verifique se o armazenamento da sessão está habilitado e tente novamente.",
    );
  }
}

function clearIdempotencyKey(idempotencyKey: string) {
  try {
    const storedIntent = sessionStorage.getItem(
      PENDING_APPOINTMENT_STORAGE_KEY,
    );
    if (!storedIntent) return;

    const parsedIntent: unknown = JSON.parse(storedIntent);
    if (
      typeof parsedIntent === "object" &&
      parsedIntent !== null &&
      "idempotencyKey" in parsedIntent &&
      parsedIntent.idempotencyKey === idempotencyKey
    ) {
      sessionStorage.removeItem(PENDING_APPOINTMENT_STORAGE_KEY);
    }
  } catch {
    // Não bloqueia a interface quando o navegador impede o acesso ao storage.
  }
}

const dateFormatter = new Intl.DateTimeFormat("pt-BR", {
  weekday: "long",
  day: "numeric",
  month: "long",
  timeZone: "America/Sao_Paulo",
});
const timeFormatter = new Intl.DateTimeFormat("pt-BR", {
  hour: "2-digit",
  minute: "2-digit",
  hour12: false,
  timeZone: "America/Sao_Paulo",
});
const currencyFormatter = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

function normalizeText(value: string) {
  return value
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .toLocaleLowerCase("pt-BR")
    .trim();
}
function normalizePhone(value: string) {
  return value.replace(/\D/g, "");
}

function formatSlot(
  slot: AppointmentSlot | null,
  procedure: ProcedimentoProfissionalApi | undefined,
) {
  if (!slot) return "Horário não selecionado";
  const date = dateFormatter.format(slot.inicio);
  const start = timeFormatter.format(slot.inicio);
  const label = `${date.charAt(0).toUpperCase() + date.slice(1)} · ${start}`;
  if (!procedure) return label;
  const end = new Date(
    slot.inicio.getTime() + procedure.duracaoEfetivaMinutos * 60_000,
  );
  return `${label} às ${timeFormatter.format(end)}`;
}

export function NewAppointmentDialog({
  open,
  onOpenChange,
  onCreated,
  slot,
  professionalName,
  professionalUuid,
}: NewAppointmentDialogProps) {
  const [clients, setClients] = useState<ClienteApi[] | null>(null);
  const [clientsError, setClientsError] = useState<string | null>(null);
  const [isLoadingClients, setIsLoadingClients] = useState(false);
  const [reloadClientsAttempt, setReloadClientsAttempt] = useState(0);
  const [procedures, setProcedures] = useState<
    ProcedimentoProfissionalApi[] | null
  >(null);
  const [proceduresProfessionalUuid, setProceduresProfessionalUuid] = useState<
    string | null
  >(null);
  const [proceduresError, setProceduresError] = useState<string | null>(null);
  const [isLoadingProcedures, setIsLoadingProcedures] = useState(false);
  const [reloadProceduresAttempt, setReloadProceduresAttempt] = useState(0);
  const [filter, setFilter] = useState<ClientFilter>("nome");
  const [query, setQuery] = useState("");
  const [motivoContato, setMotivoContato] = useState("");
  const [selectedClientUuid, setSelectedClientUuid] = useState("");
  const [selectedProcedureUuid, setSelectedProcedureUuid] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const submitLockRef = useRef(false);

  useEffect(() => {
    if (!open || clients !== null) return;
    const abortController = new AbortController();
    async function loadClients() {
      setIsLoadingClients(true);
      setClientsError(null);
      try {
        const response = await listarClientes({
          signal: abortController.signal,
        });
        if (!abortController.signal.aborted) setClients(response.clientes);
      } catch (error) {
        if (!abortController.signal.aborted)
          setClientsError(
            error instanceof Error
              ? error.message
              : "Não foi possível carregar os clientes.",
          );
      } finally {
        if (!abortController.signal.aborted) setIsLoadingClients(false);
      }
    }
    void loadClients();
    return () => abortController.abort();
  }, [clients, open, reloadClientsAttempt]);

  useEffect(() => {
    if (
      !open ||
      !professionalUuid ||
      proceduresProfessionalUuid === professionalUuid
    )
      return;
    const abortController = new AbortController();
    async function loadProcedures() {
      setIsLoadingProcedures(true);
      setProceduresError(null);
      try {
        const response = await listarProcedimentosProfissional({
          profissionalUuid: professionalUuid,
          signal: abortController.signal,
        });
        if (!abortController.signal.aborted) {
          setProcedures(response.procedimentos);
          setProceduresProfessionalUuid(professionalUuid);
        }
      } catch (error) {
        if (!abortController.signal.aborted)
          setProceduresError(
            error instanceof Error
              ? error.message
              : "Não foi possível carregar os procedimentos.",
          );
      } finally {
        if (!abortController.signal.aborted) setIsLoadingProcedures(false);
      }
    }
    void loadProcedures();
    return () => abortController.abort();
  }, [
    open,
    proceduresProfessionalUuid,
    professionalUuid,
    reloadProceduresAttempt,
  ]);

  const filteredClients = useMemo(() => {
    if (!clients) return [];
    const normalizedQuery =
      filter === "nome" ? normalizeText(query) : normalizePhone(query);
    if (!normalizedQuery) return clients;
    return clients.filter((client) =>
      filter === "nome"
        ? normalizeText(client.nome).includes(normalizedQuery)
        : normalizePhone(client.telefone).includes(normalizedQuery),
    );
  }, [clients, filter, query]);
  const availableProcedures =
    proceduresProfessionalUuid === professionalUuid ? procedures : null;
  const selectedProcedure = availableProcedures?.find(
    (procedure) =>
      procedure.profissionalProcedimentoUuid === selectedProcedureUuid,
  );
  const selectedClient = clients?.find(
    (client) => client.uuid === selectedClientUuid,
  );

  function handleOpenChange(nextOpen: boolean) {
    onOpenChange(nextOpen);
    if (!nextOpen) {
      setFilter("nome");
      setQuery("");
      setMotivoContato("");
      setSelectedClientUuid("");
      setSelectedProcedureUuid("");
    }
  }

  async function handleCreateAppointment() {
    if (submitLockRef.current) return;

    if (!slot || !selectedClient || !selectedProcedure) {
      toast.warning("Complete os dados do agendamento.", {
        description: "Selecione um procedimento, um cliente e um horário.",
      });
      return;
    }

    const agendamento = {
      clienteUuid: selectedClient.uuid,
      profissionalProcedimentoUuid:
        selectedProcedure.profissionalProcedimentoUuid,
      inicio: slot.inicio.toISOString(),
      motivoContato: motivoContato.trim() || null,
    };
    const fingerprint = JSON.stringify(agendamento);
    let idempotencyKey: string;

    try {
      idempotencyKey = getIdempotencyKey(fingerprint);
    } catch (error) {
      toast.error("Não foi possível iniciar o agendamento.", {
        description:
          error instanceof Error
            ? error.message
            : "O armazenamento da sessão está indisponível.",
      });
      return;
    }

    submitLockRef.current = true;
    setIsSubmitting(true);

    try {
      const createdAppointment = await criarAgendamento({
        agendamento,
        idempotencyKey,
      });

      clearIdempotencyKey(idempotencyKey);
      toast.success("Agendamento criado com sucesso.", {
        description: `${createdAppointment.nomeCliente} · ${createdAppointment.nomeProcedimento}`,
      });
      onCreated();
      handleOpenChange(false);
    } catch (error) {
      if (error instanceof ApiError) {
        const operationMayStillBeRunning =
          error.status === 500 ||
          error.code === "agendamento_pendente_integracao";

        if (!operationMayStillBeRunning) {
          clearIdempotencyKey(idempotencyKey);
        }

        toast.error(
          error.code === "agendamento_pendente_integracao"
            ? "O agendamento ainda está sendo processado."
            : "Não foi possível criar o agendamento.",
          {
            description: operationMayStillBeRunning
              ? `${error.message} A chave foi mantida para consultar a mesma operação ao tentar novamente.`
              : error.message,
          },
        );
      } else {
        toast.error("Não foi possível confirmar o resultado do agendamento.", {
          description:
            "O resultado não foi confirmado. Reenvie os mesmos dados para consultar a mesma operação com segurança.",
        });
      }
    } finally {
      submitLockRef.current = false;
      setIsSubmitting(false);
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={handleOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-40 bg-slate-950/40 backdrop-blur-[1px]" />
        <Dialog.Content
          onEscapeKeyDown={(event) => {
            if (isSubmitting) event.preventDefault();
          }}
          onPointerDownOutside={(event) => {
            if (isSubmitting) event.preventDefault();
          }}
          className="fixed left-1/2 top-1/2 z-50 flex max-h-[calc(100vh-3rem)] w-[min(42rem,calc(100vw-2rem))] -translate-x-1/2 -translate-y-1/2 flex-col overflow-hidden rounded-xl border border-agend-border bg-white shadow-[0_8px_30px_rgba(0,0,0,0.12)] focus:outline-none"
        >
          <header className="flex min-h-[73px] items-center justify-between border-b border-agend-border px-6 py-5">
            <div>
              <Dialog.Title className="font-heading text-xl font-semibold leading-7 text-agend-ink">
                Novo agendamento
              </Dialog.Title>
              <Dialog.Description className="mt-0.5 text-xs leading-[18px] text-agend-muted">
                Selecione o cliente para o horário escolhido na agenda.
              </Dialog.Description>
            </div>
            <Dialog.Close
              disabled={isSubmitting}
              className="grid size-8 place-items-center rounded-md text-agend-muted transition hover:bg-agend-brand-100 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
              aria-label="Fechar modal"
            >
              <X aria-hidden="true" size={18} />
            </Dialog.Close>
          </header>
          <div className="min-h-0 flex-1 overflow-y-auto p-6">
            <div className="flex flex-col gap-5">
              <section
                className="grid gap-3 rounded-lg border border-agend-border bg-agend-canvas/70 p-4 sm:grid-cols-2"
                aria-label="Detalhes do horário selecionado"
              >
                <div className="min-w-0">
                  <p className="text-[11px] font-semibold uppercase tracking-[0.08em] text-agend-subtle">
                    Profissional
                  </p>
                  <p className="mt-1 truncate text-sm font-semibold text-agend-ink">
                    {professionalName}
                  </p>
                </div>
                <div className="min-w-0 sm:border-l sm:border-agend-border sm:pl-4">
                  <p className="flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-[0.08em] text-agend-subtle">
                    <CalendarClock aria-hidden="true" size={14} />
                    Horário selecionado
                  </p>
                  <p className="mt-1 text-sm font-semibold leading-5 text-agend-ink">
                    {formatSlot(slot, selectedProcedure)}
                  </p>
                </div>
              </section>
              <section className="border-t border-agend-border pt-5">
                <div className="mb-3">
                  <h3 className="font-heading text-base font-semibold text-agend-ink">
                    Procedimento
                  </h3>
                  <p className="mt-1 text-xs leading-[18px] text-agend-muted">
                    Selecione um dos procedimentos oferecidos por este
                    profissional.
                  </p>
                </div>
                <div className="flex flex-col gap-1.5">
                  <label
                    htmlFor="appointment-procedure"
                    className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                  >
                    Procedimentos
                  </label>
                  {isLoadingProcedures ? (
                    <div className="flex min-h-10 items-center gap-2 rounded-lg border border-agend-border bg-agend-canvas px-3 text-sm text-agend-muted">
                      <LoaderCircle
                        aria-hidden="true"
                        className="animate-spin text-agend-brand-500"
                        size={16}
                      />
                      Carregando procedimentos...
                    </div>
                  ) : proceduresError ? (
                    <div
                      className="flex flex-col gap-3 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-800 sm:flex-row sm:items-center sm:justify-between"
                      role="alert"
                    >
                      <span className="flex items-start gap-2">
                        <AlertCircle
                          aria-hidden="true"
                          className="mt-0.5"
                          size={16}
                        />
                        {proceduresError}
                      </span>
                      <button
                        type="button"
                        onClick={() => {
                          setProceduresError(null);
                          setReloadProceduresAttempt((attempt) => attempt + 1);
                        }}
                        className="min-h-8 shrink-0 rounded-md border border-red-200 bg-white px-3 text-xs font-semibold text-red-800 transition hover:bg-red-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
                      >
                        Tentar novamente
                      </button>
                    </div>
                  ) : (
                    <div className="relative">
                      <select
                        id="appointment-procedure"
                        value={selectedProcedureUuid}
                        onChange={(event) =>
                          setSelectedProcedureUuid(event.target.value)
                        }
                        disabled={
                          isSubmitting ||
                          !availableProcedures ||
                          availableProcedures.length === 0
                        }
                        className="min-h-10 w-full appearance-none rounded-lg border border-agend-border bg-white px-3 pr-10 text-sm text-agend-ink outline-none transition hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 disabled:cursor-not-allowed disabled:bg-agend-canvas disabled:text-agend-subtle"
                      >
                        <option value="">
                          {!availableProcedures
                            ? "Aguardando a lista de procedimentos"
                            : availableProcedures.length === 0
                              ? "Nenhum procedimento disponível"
                              : "Selecione um procedimento"}
                        </option>
                        {availableProcedures?.map((procedure) => (
                          <option
                            key={procedure.profissionalProcedimentoUuid}
                            value={procedure.profissionalProcedimentoUuid}
                          >
                            {procedure.nome}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
                <div className="mt-4 grid gap-3 sm:grid-cols-[11.25rem_minmax(0,1fr)]">
                  <div className="flex min-w-0 flex-col gap-1.5">
                    <label
                      htmlFor="appointment-price"
                      className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                    >
                      Preço
                    </label>
                    <input
                      id="appointment-price"
                      value={
                        selectedProcedure
                          ? currencyFormatter.format(
                              selectedProcedure.valorEfetivo,
                            )
                          : ""
                      }
                      placeholder="—"
                      readOnly
                      className="min-h-10 rounded-lg border border-agend-border bg-white px-3 text-sm text-agend-ink outline-none placeholder:text-agend-subtle"
                    />
                  </div>
                  <div className="flex min-w-0 flex-col gap-1.5">
                    <label
                      htmlFor="appointment-duration"
                      className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                    >
                      Duração estimada
                    </label>
                    <input
                      id="appointment-duration"
                      value={
                        selectedProcedure
                          ? `${selectedProcedure.duracaoEfetivaMinutos} minutos`
                          : ""
                      }
                      placeholder="—"
                      readOnly
                      className="min-h-10 rounded-lg border border-agend-border bg-white px-3 text-sm text-agend-ink outline-none placeholder:text-agend-subtle"
                    />
                  </div>
                </div>
              </section>
              <section className="border-t border-agend-border pt-5">
                <div className="mb-3">
                  <h3 className="font-heading text-base font-semibold text-agend-ink">
                    Cliente
                  </h3>
                  <p className="mt-1 text-xs leading-[18px] text-agend-muted">
                    A lista é carregada uma única vez e a busca é feita neste
                    dispositivo.
                  </p>
                </div>
                <div className="grid gap-3 sm:grid-cols-[10rem_minmax(0,1fr)]">
                  <div className="flex min-w-0 flex-col gap-1.5">
                    <label
                      htmlFor="client-filter"
                      className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                    >
                      Filtrar por
                    </label>
                    <div className="relative">
                      <select
                        id="client-filter"
                        value={filter}
                        disabled={isSubmitting}
                        onChange={(event) => {
                          setFilter(event.target.value as ClientFilter);
                          setQuery("");
                        }}
                        className="min-h-10 w-full appearance-none rounded-lg border border-agend-border bg-white px-3 pr-10 text-sm text-agend-ink outline-none transition hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15"
                      >
                        <option value="nome">Nome</option>
                        <option value="telefone">Telefone</option>
                      </select>
                    </div>
                  </div>
                  <div className="flex min-w-0 flex-col gap-1.5">
                    <label
                      htmlFor="client-search"
                      className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                    >
                      Buscar cliente
                    </label>
                    <div className="relative">
                      <Search
                        aria-hidden="true"
                        className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-agend-muted"
                        size={16}
                      />
                      <input
                        id="client-search"
                        value={query}
                        onChange={(event) => setQuery(event.target.value)}
                        placeholder={
                          filter === "nome"
                            ? "Digite o nome do cliente"
                            : "Digite o telefone do cliente"
                        }
                        disabled={
                          isSubmitting ||
                          isLoadingClients ||
                          Boolean(clientsError)
                        }
                        className="min-h-10 w-full rounded-lg border border-agend-border bg-white py-2.5 pl-10 pr-4 text-sm text-agend-ink outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 disabled:cursor-not-allowed disabled:bg-agend-canvas disabled:text-agend-subtle"
                      />
                    </div>
                  </div>
                </div>
                <div className="mt-4 flex min-w-0 flex-col gap-1.5">
                  <label
                    htmlFor="appointment-client"
                    className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                  >
                    Cliente encontrado
                  </label>
                  {isLoadingClients ? (
                    <div className="flex min-h-10 items-center gap-2 rounded-lg border border-agend-border bg-agend-canvas px-3 text-sm text-agend-muted">
                      <LoaderCircle
                        aria-hidden="true"
                        className="animate-spin text-agend-brand-500"
                        size={16}
                      />
                      Carregando clientes...
                    </div>
                  ) : clientsError ? (
                    <div
                      className="flex flex-col gap-3 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-800 sm:flex-row sm:items-center sm:justify-between"
                      role="alert"
                    >
                      <span className="flex items-start gap-2">
                        <AlertCircle
                          aria-hidden="true"
                          className="mt-0.5"
                          size={16}
                        />
                        {clientsError}
                      </span>
                      <button
                        type="button"
                        onClick={() => {
                          setClientsError(null);
                          setReloadClientsAttempt((attempt) => attempt + 1);
                        }}
                        className="min-h-8 shrink-0 rounded-md border border-red-200 bg-white px-3 text-xs font-semibold text-red-800 transition hover:bg-red-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
                      >
                        Tentar novamente
                      </button>
                    </div>
                  ) : (
                    <>
                      <div className="relative">
                        <select
                          id="appointment-client"
                          value={selectedClientUuid}
                          onChange={(event) =>
                            setSelectedClientUuid(event.target.value)
                          }
                          disabled={
                            isSubmitting ||
                            !clients ||
                            filteredClients.length === 0
                          }
                          className="min-h-10 w-full appearance-none rounded-lg border border-agend-border bg-white px-3 pr-10 text-sm text-agend-ink outline-none transition hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 disabled:cursor-not-allowed disabled:bg-agend-canvas disabled:text-agend-subtle"
                        >
                          <option value="">
                            {!clients
                              ? "Aguardando a lista de clientes"
                              : filteredClients.length === 0
                                ? "Nenhum cliente encontrado"
                                : "Selecione um cliente"}
                          </option>
                          {filteredClients.map((client) => (
                            <option key={client.uuid} value={client.uuid}>
                              {client.nome} · {client.telefone}
                            </option>
                          ))}
                        </select>
                      </div>
                      {selectedClient ? (
                        <p className="text-xs leading-[18px] text-agend-muted">
                          Selecionado: {selectedClient.nome} ·{" "}
                          {selectedClient.telefone}
                        </p>
                      ) : null}
                    </>
                  )}
                </div>
              </section>
              <section className="border-t border-agend-border pt-5">
                <div className="mb-3">
                  <h3 className="font-heading text-base font-semibold text-agend-ink">
                    Motivo do contato
                  </h3>
                  <p className="mt-1 text-xs leading-[18px] text-agend-muted">
                    Campo opcional para registrar o motivo informado pelo
                    cliente.
                  </p>
                </div>
                <label htmlFor="appointment-reason" className="sr-only">
                  Motivo do contato (opcional)
                </label>
                <textarea
                  id="appointment-reason"
                  value={motivoContato}
                  onChange={(event) => setMotivoContato(event.target.value)}
                  placeholder="Descreva brevemente o motivo do agendamento"
                  rows={3}
                  disabled={isSubmitting}
                  className="min-h-22 w-full resize-y rounded-lg border border-agend-border bg-white p-3 text-sm text-agend-ink outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 disabled:cursor-not-allowed disabled:bg-agend-canvas"
                />
              </section>
            </div>
          </div>
          <footer className="flex min-h-[67px] items-center justify-end gap-3 border-t border-agend-border bg-white px-6 py-4">
            <Dialog.Close
              disabled={isSubmitting}
              className="min-h-10 rounded-lg border border-agend-border bg-white px-4 text-[13px] font-semibold text-agend-ink transition hover:border-agend-brand-500 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Cancelar
            </Dialog.Close>
            <button
              type="button"
              onClick={() => void handleCreateAppointment()}
              disabled={
                isSubmitting || !selectedClient || !selectedProcedure || !slot
              }
              className="inline-flex min-h-10 items-center justify-center gap-2 rounded-lg bg-agend-brand-500 px-4 text-[13px] font-semibold text-white transition hover:bg-agend-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSubmitting ? (
                <>
                  <LoaderCircle
                    aria-hidden="true"
                    className="animate-spin"
                    size={16}
                  />
                  Salvando...
                </>
              ) : (
                "Confirmar agendamento"
              )}
            </button>
          </footer>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
