export const AUTH_SESSION_INVALIDATED_EVENT =
  'agendai:auth-session-invalidated'

const ACCESS_TOKEN_STORAGE_KEY = 'agendai.auth.access-token.v1'

export function getAccessToken() {
  return window.sessionStorage.getItem(ACCESS_TOKEN_STORAGE_KEY)
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
