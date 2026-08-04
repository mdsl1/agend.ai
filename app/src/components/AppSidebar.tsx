import {
  CalendarDays,
  LayoutDashboard,
  PanelLeftClose,
  PanelLeftOpen,
  Settings,
  UserCog,
  UserRound,
  Users,
} from 'lucide-react'

const navigationItems = [
  { label: 'Dashboard', icon: LayoutDashboard },
  { label: 'Agenda', icon: CalendarDays, active: true },
  { label: 'Pacientes', icon: Users },
  { label: 'Usuários', icon: UserCog },
  { label: 'Configurações', icon: Settings },
]

type AppSidebarProps = {
  isOpen: boolean
  onToggle: () => void
}

export function AppSidebar({ isOpen, onToggle }: AppSidebarProps) {
  const toggleLabel = isOpen
    ? 'Recolher barra lateral'
    : 'Expandir barra lateral'

  return (
    <aside
      className={`relative z-30 flex h-screen shrink-0 flex-col overflow-hidden border-r border-agend-border bg-white transition-[width] duration-200 ease-out ${
        isOpen ? 'w-60' : 'w-18'
      }`}
      aria-label="Barra lateral"
    >
      <div
        className={`flex min-h-14 items-center border-b border-agend-border ${
          isOpen ? 'justify-between px-4' : 'justify-center px-2'
        }`}
      >
        {isOpen ? (
          <span className="whitespace-nowrap font-heading text-2xl font-bold text-agend-brand-700">
            Agend.AI
          </span>
        ) : null}

        <button
          type="button"
          onClick={onToggle}
          className={`grid size-9 shrink-0 place-items-center rounded-full text-agend-muted transition hover:bg-agend-surface-subtle hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500 ${
            isOpen ? '' : 'bg-agend-brand-100 text-agend-brand-700 shadow-sm'
          }`}
          aria-label={toggleLabel}
          aria-expanded={isOpen}
          aria-controls="app-sidebar-navigation"
          title={toggleLabel}
        >
          {isOpen ? (
            <PanelLeftClose aria-hidden="true" size={19} strokeWidth={1.8} />
          ) : (
            <PanelLeftOpen aria-hidden="true" size={19} strokeWidth={1.8} />
          )}
        </button>
      </div>

      <div
        className={`flex items-center gap-3 border-b border-agend-border ${
          isOpen ? 'justify-start p-4' : 'justify-center p-3'
        }`}
      >
        <div className="grid size-9 shrink-0 place-items-center rounded-full bg-agend-brand-100 text-agend-brand-700">
          <UserRound aria-hidden="true" size={19} strokeWidth={1.8} />
        </div>
        {isOpen ? (
          <div className="min-w-0">
            <p className="truncate text-[13px] font-semibold text-agend-ink">
              Marina Silva
            </p>
            <p className="text-xs text-agend-muted">Atendente</p>
          </div>
        ) : null}
      </div>

      <nav
        id="app-sidebar-navigation"
        className="flex flex-1 flex-col gap-1 overflow-y-auto px-2 py-4"
        aria-label="Navegação principal"
      >
        {navigationItems.map(({ label, icon: Icon, active }) => (
          <button
            key={label}
            type="button"
            className={`flex min-h-11 w-full items-center gap-3 rounded-lg border-l-4 px-3 text-[13px] font-semibold transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500 ${
              isOpen ? 'justify-start pl-4' : 'justify-center'
            } ${
              active
                ? 'border-agend-brand-700 bg-agend-brand-100 text-agend-brand-700'
                : 'border-transparent text-agend-muted hover:bg-agend-brand-100/40 hover:text-agend-brand-700'
            }`}
            aria-current={active ? 'page' : undefined}
            title={label}
          >
            <Icon
              aria-hidden="true"
              className="size-5 shrink-0"
              strokeWidth={1.8}
            />
            {isOpen ? <span>{label}</span> : <span className="sr-only">{label}</span>}
          </button>
        ))}
      </nav>

      <div
        className={`border-t border-agend-border px-3 py-4 text-[11px] text-agend-subtle ${
          isOpen ? 'text-left' : 'text-center'
        }`}
      >
        {isOpen ? 'PoC · Agenda estática' : 'PoC'}
      </div>
    </aside>
  )
}
