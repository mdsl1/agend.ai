import { apiFetch, ensureApiSuccess, isRecord } from '../api/apiClient'

export type EventoAgendaApi = {
  id: string
  titulo: string
  tipo: 'agendamento' | 'indisponibilidade'
  agendamentoUuid: string | null
  inicio: string
  fim: string
  nomeCliente: string | null
  nomeProcedimento: string | null
  profissionalUuid: string
}

export type EspecialidadeProfissionalApi = {
  uuid: string
  nome: string
}

export type ProfissionalAgendavelApi = {
  profissionalUuid: string
  nomeExibicao: string
  especialidade: EspecialidadeProfissionalApi | null
}

type ConsultarAgendaResponse = {
  eventos: EventoAgendaApi[]
}

type ListarProfissionaisResponse = {
  profissionais: ProfissionalAgendavelApi[]
}

type ConsultarAgendaParams = {
  profissionalUuid: string
  inicio: string
  fim: string
  signal?: AbortSignal
}

type ListarProfissionaisParams = {
  signal?: AbortSignal
}

function isEventoAgenda(value: unknown): value is EventoAgendaApi {
  if (!isRecord(value)) {
    return false
  }

  return (
    typeof value.id === 'string' &&
    typeof value.titulo === 'string' &&
    (value.tipo === 'agendamento' || value.tipo === 'indisponibilidade') &&
    (typeof value.agendamentoUuid === 'string' ||
      value.agendamentoUuid === null) &&
    typeof value.inicio === 'string' &&
    typeof value.fim === 'string' &&
    (typeof value.nomeCliente === 'string' || value.nomeCliente === null) &&
    (typeof value.nomeProcedimento === 'string' ||
      value.nomeProcedimento === null) &&
    typeof value.profissionalUuid === 'string'
  )
}

function isEspecialidadeProfissional(
  value: unknown,
): value is EspecialidadeProfissionalApi {
  return (
    isRecord(value) &&
    typeof value.uuid === 'string' &&
    typeof value.nome === 'string'
  )
}

function isProfissionalAgendavel(
  value: unknown,
): value is ProfissionalAgendavelApi {
  return (
    isRecord(value) &&
    typeof value.profissionalUuid === 'string' &&
    typeof value.nomeExibicao === 'string' &&
    (value.especialidade === null ||
      isEspecialidadeProfissional(value.especialidade))
  )
}

function isConsultarAgendaResponse(
  value: unknown,
): value is ConsultarAgendaResponse {
  return (
    isRecord(value) &&
    Array.isArray(value.eventos) &&
    value.eventos.every(isEventoAgenda)
  )
}

function isListarProfissionaisResponse(
  value: unknown,
): value is ListarProfissionaisResponse {
  return (
    isRecord(value) &&
    Array.isArray(value.profissionais) &&
    value.profissionais.every(isProfissionalAgendavel)
  )
}

export async function listarProfissionaisAgendaveis({
  signal,
}: ListarProfissionaisParams): Promise<ListarProfissionaisResponse> {
  const response = await apiFetch('/api/profissionais', { signal })

  await ensureApiSuccess(
    response,
    `Não foi possível carregar os profissionais (HTTP ${response.status}).`,
  )

  const data: unknown = await response.json()

  if (!isListarProfissionaisResponse(data)) {
    throw new Error('A API retornou um formato de profissionais inválido.')
  }

  return data
}

export async function consultarAgenda({
  profissionalUuid,
  inicio,
  fim,
  signal,
}: ConsultarAgendaParams): Promise<ConsultarAgendaResponse> {
  const url = new URL(
    `/api/agenda/${encodeURIComponent(profissionalUuid)}`,
    window.location.origin,
  )
  url.searchParams.set('inicio', inicio)
  url.searchParams.set('fim', fim)

  const response = await apiFetch(url, { signal })

  await ensureApiSuccess(
    response,
    `Não foi possível consultar a agenda (HTTP ${response.status}).`,
  )

  const data: unknown = await response.json()

  if (!isConsultarAgendaResponse(data)) {
    throw new Error('A API retornou um formato de agenda inválido.')
  }

  return data
}
