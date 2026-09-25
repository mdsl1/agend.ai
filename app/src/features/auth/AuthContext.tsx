import {
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { toast } from 'sonner'
import { ApiError } from '../api/apiClient'
import { fazerLogin, obterMeuPerfil, type MeuPerfilApi } from './authApi'
import {
  AuthContext,
  type AuthStatus,
  type SignInParams,
} from './authContextValue'
import {
  AUTH_SESSION_INVALIDATED_EVENT,
  clearAccessToken,
  getAccessToken,
  saveAccessToken,
} from './authSession'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<MeuPerfilApi | null>(null)
  const [status, setStatus] = useState<AuthStatus>(() =>
    getAccessToken() ? 'loading' : 'anonymous',
  )

  useEffect(() => {
    const handleInvalidSession = () => {
      setUser(null)
      setStatus('anonymous')
      toast.warning('Sua sessão expirou.', {
        description: 'Entre novamente para continuar usando o Agend.AI.',
      })
    }

    window.addEventListener(
      AUTH_SESSION_INVALIDATED_EVENT,
      handleInvalidSession,
    )

    return () =>
      window.removeEventListener(
        AUTH_SESSION_INVALIDATED_EVENT,
        handleInvalidSession,
      )
  }, [])

  useEffect(() => {
    if (!getAccessToken()) {
      return
    }

    const abortController = new AbortController()

    async function restoreSession() {
      try {
        const profile = await obterMeuPerfil({
          signal: abortController.signal,
        })

        if (!abortController.signal.aborted) {
          setUser(profile)
          setStatus('authenticated')
        }
      } catch (error) {
        if (abortController.signal.aborted) {
          return
        }

        clearAccessToken()
        setUser(null)
        setStatus('anonymous')

        if (!(error instanceof ApiError && error.status === 401)) {
          toast.error('Não foi possível restaurar sua sessão.', {
            description:
              error instanceof Error
                ? error.message
                : 'Tente entrar novamente.',
          })
        }
      }
    }

    void restoreSession()

    return () => abortController.abort()
  }, [])

  async function signIn(params: SignInParams) {
    const login = await fazerLogin(params)
    saveAccessToken(login.accessToken)

    try {
      const profile = await obterMeuPerfil({ signal: params.signal })
      setUser(profile)
      setStatus('authenticated')
    } catch (error) {
      clearAccessToken()
      setUser(null)
      setStatus('anonymous')
      throw error
    }
  }

  function signOut() {
    clearAccessToken()
    setUser(null)
    setStatus('anonymous')
    toast.success('Sessão encerrada com segurança.')
  }

  const value = useMemo(
    () => ({ status, user, signIn, signOut }),
    [status, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
