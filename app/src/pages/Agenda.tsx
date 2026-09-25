import { useState } from 'react'
import { AppSidebar } from '../components/AppSidebar'
import { Header } from '../components/Header'
import { WeeklyAgenda } from '../components/WeeklyAgenda'
import { getDisplayedRole } from '../features/auth/profilePresentation'
import { useAuth } from '../features/auth/useAuth'

export function Agenda() {
  const { user, signOut } = useAuth()
  const [isSidebarOpen, setIsSidebarOpen] = useState(() =>
    window.matchMedia('(min-width: 1024px)').matches,
  )

  if (!user) {
    return null
  }

  const displayName = user.prefixo
    ? `${user.prefixo} ${user.nome}`
    : user.nome
  const displayRole = getDisplayedRole(user)

  return (
    <div className="flex min-h-screen min-w-0 overflow-hidden bg-agend-canvas text-agend-ink">
      <AppSidebar
        isOpen={isSidebarOpen}
        onToggle={() => setIsSidebarOpen((isOpen) => !isOpen)}
        userName={displayName}
        userRole={displayRole}
      />

      <div className="flex h-screen min-w-0 flex-1 flex-col">
        <Header userName={user.nome} onLogout={signOut} />

        <main className="min-h-0 flex-1 overflow-auto px-6 py-4 sm:py-6 lg:px-8 2xl:px-10">
          <div className="flex min-h-full w-full min-w-0 flex-col gap-6">
            <WeeklyAgenda />
          </div>
        </main>
      </div>
    </div>
  )
}
