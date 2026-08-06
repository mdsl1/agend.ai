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

        <main className="min-h-0 flex-1 overflow-auto px-6 py-4 sm:py-6 lg:px-8 2xl:px-10">
          <div className="flex min-h-full w-full min-w-0 flex-col gap-6">
            <WeeklyAgenda />
          </div>
        </main>
      </div>

      <Toaster position="top-right" richColors closeButton />
    </div>
  )
}

export default App
