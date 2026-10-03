import { apiFetch, ensureApiSuccess, isRecord } from '../api/apiClient'

export type ClienteApi = {
  uuid: string
  nome: string
  telefone: string
  email: string | null
  dataNascimento: string | null
  genero: string | null
}

type ListarClientesResponse = {
  clientes: ClienteApi[]
}

type ListarClientesParams = {
  signal?: AbortSignal
}

function isCliente(value: unknown): value is ClienteApi {
  return (
    isRecord(value) &&
    typeof value.uuid === 'string' &&
    typeof value.nome === 'string' &&
    typeof value.telefone === 'string' &&
    (typeof value.email === 'string' || value.email === null) &&
    (typeof value.dataNascimento === 'string' ||
      value.dataNascimento === null) &&
    (typeof value.genero === 'string' || value.genero === null)
  )
}

function isListarClientesResponse(
  value: unknown,
): value is ListarClientesResponse {
  return (
    isRecord(value) &&
    Array.isArray(value.clientes) &&
    value.clientes.every(isCliente)
  )
}

export async function listarClientes({
  signal,
}: ListarClientesParams = {}): Promise<ListarClientesResponse> {
  const response = await apiFetch('/api/clientes', { signal })

  await ensureApiSuccess(
    response,
    `Não foi possível carregar os clientes (HTTP ${response.status}).`,
  )

  const data: unknown = await response.json()

  if (!isListarClientesResponse(data)) {
    throw new Error('A API retornou um formato de clientes inválido.')
  }

  return data
}
