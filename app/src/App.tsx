import { useEffect } from 'react'
import { LoaderCircle } from 'lucide-react'
import { Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom'
import { Toaster } from 'sonner'
import {
  getClinicLoginPath,
  getStoredClinicUuid,
  normalizeClinicUuid,
  saveClinicUuid,
} from './features/auth/clinicContext'
import { useAuth } from './features/auth/useAuth'
import { Agenda } from './pages/Agenda'
import { Login } from './pages/Login'

function MissingClinicContext() {
  return (
    <main className="grid min-h-screen place-items-center bg-agend-canvas px-6 text-agend-ink">
      <section
        aria-labelledby="missing-clinic-title"
        className="w-full max-w-md rounded-xl border border-agend-border bg-white p-8 text-center shadow-agend-card"
      >
        <h1
          id="missing-clinic-title"
          className="font-heading text-2xl font-bold"
        >
          Clínica não identificada
        </h1>
        <p className="mt-3 text-sm leading-6 text-agend-muted">
          Abra o link de acesso fornecido pela sua clínica para entrar no Agend.AI.
        </p>
      </section>
    </main>
  )
}

function ClinicLoginRoute() {
  const { clinicaUuid: routeClinicUuid } = useParams()
  const clinicUuid = normalizeClinicUuid(routeClinicUuid)

  useEffect(() => {
    if (clinicUuid) {
      saveClinicUuid(clinicUuid)
    }
  }, [clinicUuid])

  return clinicUuid ? <Login clinicaUuid={clinicUuid} /> : <MissingClinicContext />
}

function LegacyLoginRoute() {
  const clinicUuid = getStoredClinicUuid()

  return clinicUuid ? (
    <Navigate replace to={`/${clinicUuid}/login`} />
  ) : (
    <MissingClinicContext />
  )
}

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
  const location = useLocation()
  const clinicUuidFromLoginPath = normalizeClinicUuid(
    location.pathname.match(/^\/([^/]+)\/login\/?$/)?.[1],
  )

  useEffect(() => {
    if (clinicUuidFromLoginPath) {
      saveClinicUuid(clinicUuidFromLoginPath)
    }
  }, [clinicUuidFromLoginPath])

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
            isAuthenticated ? <Navigate replace to="/agenda" /> : <LegacyLoginRoute />
          }
        />
        <Route
          path="/:clinicaUuid/login"
          element={
            isAuthenticated ? <Navigate replace to="/agenda" /> : <ClinicLoginRoute />
          }
        />
        <Route
          path="/contexto-clinica-ausente"
          element={isAuthenticated ? <Navigate replace to="/agenda" /> : <MissingClinicContext />}
        />
        <Route
          path="/agenda"
          element={
            isAuthenticated ? <Agenda /> : <Navigate replace to={getClinicLoginPath()} />
          }
        />
        <Route
          path="*"
          element={
            <Navigate replace to={isAuthenticated ? '/agenda' : getClinicLoginPath()} />
          }
        />
      </Routes>

      <Toaster position="top-right" richColors closeButton />
    </>
  )
}

export default App
