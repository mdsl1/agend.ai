import { apiFetch, ensureApiSuccess, isRecord } from '../api/apiClient'

export type UsuarioLoginApi = {
  uuid: string
  nome: string
  cargo: string
  isAdmin: boolean
  profissionalUuid: string | null
  clinicaUuid: string
}

export type LoginResponseApi = {
  accessToken: string
  expiraEm: string
  usuario: UsuarioLoginApi
}

export type ClinicaUsuarioApi = {
  uuid: string
  nome: string
  tipoClinica: 'medica' | 'odontologica' | 'estetica'
}

export type MeuPerfilApi = {
  usuarioUuid: string
  profissionalUuid: string | null
  nome: string
  prefixo: string | null
  email: string
  cargo: string
  isAdmin: boolean
  registroProfissional: string | null
  especialidade: string | null
  clinica: ClinicaUsuarioApi
  permissoes: string[]
}

type LoginParams = {
  clinicaUuid: string
  email: string
  senha: string
  signal?: AbortSignal
}

type MeuPerfilParams = {
  signal?: AbortSignal
}

function isUsuarioLogin(value: unknown): value is UsuarioLoginApi {
  return (
    isRecord(value) &&
    typeof value.uuid === 'string' &&
    typeof value.nome === 'string' &&
    typeof value.cargo === 'string' &&
    typeof value.isAdmin === 'boolean' &&
    (typeof value.profissionalUuid === 'string' ||
      value.profissionalUuid === null) &&
    typeof value.clinicaUuid === 'string'
  )
}

function isLoginResponse(value: unknown): value is LoginResponseApi {
  return (
    isRecord(value) &&
    typeof value.accessToken === 'string' &&
    typeof value.expiraEm === 'string' &&
    isUsuarioLogin(value.usuario)
  )
}

function isClinicaUsuario(value: unknown): value is ClinicaUsuarioApi {
  return (
    isRecord(value) &&
    typeof value.uuid === 'string' &&
    typeof value.nome === 'string' &&
    (value.tipoClinica === 'medica' ||
      value.tipoClinica === 'odontologica' ||
      value.tipoClinica === 'estetica')
  )
}

function isMeuPerfil(value: unknown): value is MeuPerfilApi {
  return (
    isRecord(value) &&
    typeof value.usuarioUuid === 'string' &&
    (typeof value.profissionalUuid === 'string' ||
      value.profissionalUuid === null) &&
    typeof value.nome === 'string' &&
    (typeof value.prefixo === 'string' || value.prefixo === null) &&
    typeof value.email === 'string' &&
    typeof value.cargo === 'string' &&
    typeof value.isAdmin === 'boolean' &&
    (typeof value.registroProfissional === 'string' ||
      value.registroProfissional === null) &&
    (typeof value.especialidade === 'string' ||
      value.especialidade === null) &&
    isClinicaUsuario(value.clinica) &&
    Array.isArray(value.permissoes) &&
    value.permissoes.every((permissao) => typeof permissao === 'string')
  )
}

export async function fazerLogin({
  clinicaUuid,
  email,
  senha,
  signal,
}: LoginParams): Promise<LoginResponseApi> {
  const response = await apiFetch(
    '/api/auth/login',
    {
      method: 'POST',
      body: JSON.stringify({ clinicaUuid, email, senha }),
      signal,
    },
    { authenticated: false },
  )

  await ensureApiSuccess(
    response,
    `Não foi possível entrar no sistema (HTTP ${response.status}).`,
  )

  const data: unknown = await response.json()

  if (!isLoginResponse(data)) {
    throw new Error('A API retornou um formato de login inválido.')
  }

  return data
}

export async function obterMeuPerfil({
  signal,
}: MeuPerfilParams = {}): Promise<MeuPerfilApi> {
  const response = await apiFetch('/api/me', { signal })

  await ensureApiSuccess(
    response,
    `Não foi possível carregar o perfil atual (HTTP ${response.status}).`,
  )

  const data: unknown = await response.json()

  if (!isMeuPerfil(data)) {
    throw new Error('A API retornou um formato de perfil inválido.')
  }

  return data
}
