import { createContext } from 'react'
import type { MeuPerfilApi } from './authApi'

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous'

export type SignInParams = {
  clinicaUuid: string
  email: string
  senha: string
  signal?: AbortSignal
}

export type AuthContextValue = {
  status: AuthStatus
  user: MeuPerfilApi | null
  signIn: (params: SignInParams) => Promise<void>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
