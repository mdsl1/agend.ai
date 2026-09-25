import { LoaderCircle } from 'lucide-react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { Toaster } from 'sonner'
import { useAuth } from './features/auth/useAuth'
import { Agenda } from './pages/Agenda'
import { Login } from './pages/Login'

function SessionLoading() {
  return (
    <main
      className="grid min-h-screen place-items-center bg-agend-canvas text-agend-muted"
      aria-busy="true"
      aria-label="Validando sessão"
    >
      <div className="flex items-center gap-3 text-sm font-medium">
        <LoaderCircle
          aria-hidden="true"
          className="animate-spin text-agend-brand-500"
          size={20}
        />
        Validando sua sessão...
      </div>
    </main>
  )
}

function App() {
  const { status } = useAuth()

  if (status === 'loading') {
    return (
      <>
        <SessionLoading />
        <Toaster position="top-right" richColors closeButton />
      </>
    )
  }

  const isAuthenticated = status === 'authenticated'

  return (
    <>
      <Routes>
        <Route
          path="/login"
          element={
            isAuthenticated ? <Navigate replace to="/agenda" /> : <Login />
          }
        />
        <Route
          path="/agenda"
          element={
            isAuthenticated ? <Agenda /> : <Navigate replace to="/login" />
          }
        />
        <Route
          path="*"
          element={
            <Navigate replace to={isAuthenticated ? '/agenda' : '/login'} />
          }
        />
      </Routes>

      <Toaster position="top-right" richColors closeButton />
    </>
  )
}

export default App
