import { Bell, LogOut } from 'lucide-react'

type HeaderProps = {
  userName: string
  onLogout: () => void
}

export function Header({ userName, onLogout }: HeaderProps) {
  const firstName = userName.trim().split(/\s+/)[0] || userName

  return (
    <header className="relative z-20 flex min-h-14 items-center justify-between border-b border-agend-border-strong bg-agend-brand-600 px-5 text-agend-on-brand shadow-agend-header sm:px-8 lg:px-12">
      <div>
        <p className="font-heading text-lg font-bold leading-tight sm:text-xl">
          Olá, {firstName}
        </p>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          className="grid size-9 place-items-center rounded-full text-white/90 transition hover:bg-white/15 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
          aria-label="Ver notificações"
        >
          <Bell aria-hidden="true" size={18} strokeWidth={1.8} />
        </button>
        <button
          type="button"
          onClick={onLogout}
          className="grid size-9 place-items-center rounded-full text-white/90 transition hover:bg-white/15 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
          aria-label="Sair do sistema"
        >
          <LogOut aria-hidden="true" size={18} strokeWidth={1.8} />
        </button>
      </div>
    </header>
  )
}
