export type EventoAgendaApi = {
  id: string
  titulo: string
  inicio: string
  fim: string
  nomeCliente: string
  nomeProcedimento: string | null
  profissionalUuid: string
}

export type AgendaDisponivelApi = {
  profissionalUuid: string
  nomeExibicao: string
}

type ConsultarAgendaResponse = {
  eventos: EventoAgendaApi[]
}

type ListarAgendasResponse = {
  agendas: AgendaDisponivelApi[]
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

type ListarAgendasParams = {
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
    typeof value.inicio === 'string' &&
    typeof value.fim === 'string' &&
    typeof value.nomeCliente === 'string' &&
    (typeof value.nomeProcedimento === 'string' ||
      value.nomeProcedimento === null) &&
    typeof value.profissionalUuid === 'string'
  )
}

function isAgendaDisponivel(value: unknown): value is AgendaDisponivelApi {
  return (
    isRecord(value) &&
    typeof value.profissionalUuid === 'string' &&
    typeof value.nomeExibicao === 'string'
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

function isListarAgendasResponse(value: unknown): value is ListarAgendasResponse {
  return (
    isRecord(value) &&
    Array.isArray(value.agendas) &&
    value.agendas.every(isAgendaDisponivel)
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

export async function listarAgendasAtivas({
  clinicaUuid,
  signal,
}: ListarAgendasParams): Promise<ListarAgendasResponse> {
  const url = new URL('/api/agendas', window.location.origin)
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

  if (!isListarAgendasResponse(data)) {
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
