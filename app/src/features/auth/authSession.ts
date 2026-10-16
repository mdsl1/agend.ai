export const AUTH_SESSION_INVALIDATED_EVENT =
  'agendai:auth-session-invalidated'

const ACCESS_TOKEN_STORAGE_KEY = 'agendai.auth.access-token.v1'

export function getAccessToken() {
  return window.sessionStorage.getItem(ACCESS_TOKEN_STORAGE_KEY)
}

export function getAccessTokenExpiration() {
  const accessToken = getAccessToken()
  const encodedPayload = accessToken?.split('.')[1]

  if (!encodedPayload) {
    return null
  }

  try {
    const normalizedPayload = encodedPayload
      .replace(/-/g, '+')
      .replace(/_/g, '/')
      .padEnd(Math.ceil(encodedPayload.length / 4) * 4, '=')
    const payload: unknown = JSON.parse(window.atob(normalizedPayload))

    if (
      typeof payload === 'object' &&
      payload !== null &&
      'exp' in payload &&
      typeof payload.exp === 'number'
    ) {
      return payload.exp * 1000
    }
  } catch {
    // A API continua sendo a autoridade; tokens ilegíveis são verificados pelo endpoint protegido.
  }

  return null
}

export function saveAccessToken(accessToken: string) {
  window.sessionStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, accessToken)
}

export function clearAccessToken() {
  window.sessionStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY)
}

export function invalidateAuthSession() {
  clearAccessToken()
  window.dispatchEvent(new Event(AUTH_SESSION_INVALIDATED_EVENT))
}
