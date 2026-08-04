import { useState } from 'react'
import { Toaster } from 'sonner'
import { AppSidebar } from './components/AppSidebar'
import { Header } from './components/Header'
import { WeeklyAgenda } from './components/WeeklyAgenda'

function App() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(() =>
    window.matchMedia('(min-width: 1024px)').matches,
  )

  return (
    <div className="flex min-h-screen min-w-0 overflow-hidden bg-agend-canvas text-agend-ink">
      <AppSidebar
        isOpen={isSidebarOpen}
        onToggle={() => setIsSidebarOpen((isOpen) => !isOpen)}
      />

      <div className="flex h-screen min-w-0 flex-1 flex-col">
        <Header />

        <main className="min-h-0 flex-1 overflow-auto p-4 sm:p-6">
          <div className="mx-auto flex w-full max-w-360 flex-col gap-6">
            <WeeklyAgenda />
          </div>
        </main>
      </div>

      <Toaster position="top-right" richColors closeButton />
    </div>
  )
}

export default App
