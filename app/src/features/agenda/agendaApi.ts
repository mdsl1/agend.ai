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

type ApiProblemDetails = {
  detail?: string
  title?: string
}

type ConsultarAgendaParams = {
  profissionalUuid: string
  inicio: string
  fim: string
  signal?: AbortSignal
}

type ListarProfissionaisParams = {
  clinicaUuid: string
  signal?: AbortSignal
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
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

async function getApiError(response: Response) {
  try {
    const problem: unknown = await response.json()

    if (isRecord(problem)) {
      const { detail, title } = problem as ApiProblemDetails
      return detail ?? title
    }
  } catch {
    // A API pode responder sem um corpo JSON em falhas de infraestrutura.
  }

  return undefined
}

export async function listarProfissionaisAgendaveis({
  clinicaUuid,
  signal,
}: ListarProfissionaisParams): Promise<ListarProfissionaisResponse> {
  const url = new URL('/api/profissionais', window.location.origin)
  url.searchParams.set('clinicaUuid', clinicaUuid)

  const response = await fetch(url, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    const apiMessage = await getApiError(response)
    throw new Error(
      apiMessage ??
        `Não foi possível carregar as agendas (HTTP ${response.status}).`,
    )
  }

  const data: unknown = await response.json()

  if (!isListarProfissionaisResponse(data)) {
    throw new Error('A API retornou um formato de agendas inválido.')
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

  const response = await fetch(url, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    const apiMessage = await getApiError(response)
    throw new Error(
      apiMessage ?? `Não foi possível consultar a agenda (HTTP ${response.status}).`,
    )
  }

  const data: unknown = await response.json()

  if (!isConsultarAgendaResponse(data)) {
    throw new Error('A API retornou um formato de agenda inválido.')
  }

  return data
}
