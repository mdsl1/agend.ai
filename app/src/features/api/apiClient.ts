import {
  getAccessToken,
  invalidateAuthSession,
} from '../auth/authSession'

type ApiProblemDetails = {
  codigo?: string
  detail?: string
  title?: string
}

type ApiFetchOptions = {
  authenticated?: boolean
}

export class ApiError extends Error {
  readonly status: number
  readonly code?: string

  constructor(message: string, status: number, code?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

export async function apiFetch(
  input: RequestInfo | URL,
  init: RequestInit = {},
  { authenticated = true }: ApiFetchOptions = {},
) {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')

  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (authenticated) {
    const accessToken = getAccessToken()

    if (accessToken) {
      headers.set('Authorization', `Bearer ${accessToken}`)
    }
  }

  const response = await fetch(input, { ...init, headers })

  if (authenticated && response.status === 401) {
    invalidateAuthSession()
  }

  return response
}

export async function ensureApiSuccess(
  response: Response,
  fallbackMessage: string,
) {
  if (response.ok) {
    return
  }

  let problem: ApiProblemDetails | undefined

  try {
    const body: unknown = await response.json()

    if (isRecord(body)) {
      problem = body as ApiProblemDetails
    }
  } catch {
    // Falhas de infraestrutura podem não devolver Problem Details em JSON.
  }

  throw new ApiError(
    problem?.detail ?? problem?.title ?? fallbackMessage,
    response.status,
    problem?.codigo,
  )
}
