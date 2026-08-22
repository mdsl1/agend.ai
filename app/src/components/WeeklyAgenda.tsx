import { useEffect, useRef, useState } from 'react'
import type { ChangeEvent } from 'react'
import type {
  DatesSetArg,
  DayHeaderContentArg,
  EventContentArg,
  EventInput,
} from '@fullcalendar/core'
import ptBrLocale from '@fullcalendar/core/locales/pt-br'
import dayGridPlugin from '@fullcalendar/daygrid'
import luxonPlugin from '@fullcalendar/luxon3'
import FullCalendar from '@fullcalendar/react'
import timeGridPlugin from '@fullcalendar/timegrid'
import { toast } from 'sonner'
import {
  AlertCircle,
  CalendarDays,
  CalendarRange,
  ChevronLeft,
  ChevronRight,
  Columns3,
  Grid3X3,
  LoaderCircle,
  Plus,
  RefreshCw,
  Stethoscope,
} from 'lucide-react'
import {
  consultarAgenda,
  listarAgendasAtivas,
  type AgendaDisponivelApi,
  type EventoAgendaApi,
} from '../features/agenda/agendaApi'

type AppointmentDetails = {
  patient: string
  procedure: string
  doctorUuid: string
}

type CalendarView = 'timeGridWeek' | 'timeGridWorkWeek' | 'dayGridMonth'

const MIN_CALENDAR_HEIGHT = 608
const POC_CLINIC_UUID = '90d45ab0-d7d9-46ed-88b6-3529a92a15ff'

const calendarViews: Array<{
  id: CalendarView
  label: string
  icon: typeof CalendarRange
}> = [
  { id: 'timeGridWeek', label: 'Semana', icon: Columns3 },
  { id: 'timeGridWorkWeek', label: 'Semana útil', icon: CalendarRange },
  { id: 'dayGridMonth', label: 'Mês', icon: Grid3X3 },
]

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: 'numeric',
  month: 'long',
  timeZone: 'America/Sao_Paulo',
})

const monthFormatter = new Intl.DateTimeFormat('pt-BR', {
  month: 'long',
  year: 'numeric',
  timeZone: 'America/Sao_Paulo',
})

const weekdayFormatter = new Intl.DateTimeFormat('pt-BR', {
  weekday: 'short',
  timeZone: 'America/Sao_Paulo',
})

const dayNumberFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: 'numeric',
  timeZone: 'America/Sao_Paulo',
})

const initialSelectedDate = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'America/Sao_Paulo',
}).format(new Date())

function formatVisibleRange(start: Date, exclusiveEnd: Date) {
  const end = new Date(exclusiveEnd)
  end.setDate(end.getDate() - 1)

  const startParts = dateFormatter.formatToParts(start)
  const endParts = dateFormatter.formatToParts(end)
  const startDay = startParts.find((part) => part.type === 'day')?.value
  const startMonth = startParts.find((part) => part.type === 'month')?.value
  const endDay = endParts.find((part) => part.type === 'day')?.value
  const endMonth = endParts.find((part) => part.type === 'month')?.value

  if (startMonth === endMonth) {
    return `${startDay} — ${endDay} de ${endMonth} de ${end.getFullYear()}`
  }

  return `${startDay} de ${startMonth} — ${endDay} de ${endMonth} de ${end.getFullYear()}`
}

function formatVisiblePeriod({ start, end, view }: DatesSetArg) {
  if (view.type === 'dayGridMonth') {
    const monthLabel = monthFormatter.format(view.currentStart)
    return monthLabel.charAt(0).toUpperCase() + monthLabel.slice(1)
  }

  if (view.type === 'timeGridWorkWeek') {
    const workWeekEnd = new Date(start)
    workWeekEnd.setDate(workWeekEnd.getDate() + 5)
    return formatVisibleRange(start, workWeekEnd)
  }

  return formatVisibleRange(start, end)
}

function toFullCalendarEvent(event: EventoAgendaApi): EventInput {
  return {
    id: event.id,
    title: event.titulo,
    start: event.inicio,
    end: event.fim,
    extendedProps: {
      patient: event.nomeCliente || event.titulo,
      procedure: event.nomeProcedimento ?? 'Procedimento não informado',
      doctorUuid: event.profissionalUuid,
    } satisfies AppointmentDetails,
  }
}

function AppointmentCard({ event, view }: EventContentArg) {
  const details = event.extendedProps as AppointmentDetails
  const isMonthView = view.type === 'dayGridMonth'

  return (
    <div
      className={`h-full min-h-0 overflow-hidden rounded border-agend-brand-700 bg-agend-brand-100 text-agend-brand-700 ring-1 ring-inset ring-agend-brand-500/15 ${
        isMonthView
          ? 'border-l-2 px-1.5 py-1'
          : 'border-l-4 px-2 py-1.5'
      }`}
    >
      <p
        className={`truncate font-semibold text-agend-ink ${isMonthView ? 'text-[10px]' : 'text-xs'}`}
      >
        {details.patient}
      </p>
      {isMonthView ? null : (
        <p className="truncate text-[10px] leading-4 text-agend-muted">
          {details.procedure}
        </p>
      )}
    </div>
  )
}

function CalendarDayHeader({ date, isToday, view }: DayHeaderContentArg) {
  const weekday = weekdayFormatter.format(date).replace('.', '').toUpperCase()

  if (view.type === 'dayGridMonth') {
    return <span>{weekday}</span>
  }

  return (
    <div className="flex min-h-14 flex-col items-center justify-center gap-0.5 py-2">
      <span
        className={`text-[10px] font-semibold uppercase tracking-[0.08em] ${
          isToday ? 'text-agend-brand-700' : 'text-agend-muted'
        }`}
      >
        {weekday}
      </span>
      <span
        className={`font-heading text-base font-bold leading-none ${
          isToday ? 'text-agend-brand-700' : 'text-agend-ink'
        }`}
      >
        {dayNumberFormatter.format(date)}
      </span>
    </div>
  )
}

export function WeeklyAgenda() {
  const calendarRef = useRef<FullCalendar>(null)
  const calendarContainerRef = useRef<HTMLDivElement>(null)
  const [agendas, setAgendas] = useState<AgendaDisponivelApi[]>([])
  const [selectedDoctor, setSelectedDoctor] = useState('')
  const [selectedDate, setSelectedDate] = useState(initialSelectedDate)
  const [calendarView, setCalendarView] = useState<CalendarView>('timeGridWeek')
  const [calendarHeight, setCalendarHeight] = useState(MIN_CALENDAR_HEIGHT)
  const [appointments, setAppointments] = useState<EventInput[]>([])
  const [queryPeriod, setQueryPeriod] = useState<{
    inicio: string
    fim: string
  } | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [reloadAttempt, setReloadAttempt] = useState(0)
  const [isLoadingAgendas, setIsLoadingAgendas] = useState(true)
  const [agendasError, setAgendasError] = useState<string | null>(null)
  const [reloadAgendasAttempt, setReloadAgendasAttempt] = useState(0)
  const [visibleRange, setVisibleRange] = useState('Carregando período...')

  const activeDoctor = agendas.find(
    (agenda) => agenda.profissionalUuid === selectedDoctor,
  )

  useEffect(() => {
    const abortController = new AbortController()

    async function loadAgendas() {
      setIsLoadingAgendas(true)
      setAgendasError(null)

      try {
        const response = await listarAgendasAtivas({
          clinicaUuid: POC_CLINIC_UUID,
          signal: abortController.signal,
        })

        if (abortController.signal.aborted) {
          return
        }

        setAgendas(response.agendas)
        if (response.agendas.length === 0) {
          setAppointments([])
          setIsLoading(false)
          setLoadError(null)
        }
        setSelectedDoctor((currentDoctor) => {
          const currentDoctorIsAvailable = response.agendas.some(
            (agenda) => agenda.profissionalUuid === currentDoctor,
          )

          return currentDoctorIsAvailable
            ? currentDoctor
            : (response.agendas[0]?.profissionalUuid ?? '')
        })
      } catch (error) {
        if (abortController.signal.aborted) {
          return
        }

        setAgendas([])
        setSelectedDoctor('')
        setAppointments([])
        setIsLoading(false)
        setLoadError(null)
        setAgendasError(
          error instanceof Error
            ? error.message
            : 'Não foi possível carregar as agendas ativas.',
        )
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoadingAgendas(false)
        }
      }
    }

    void loadAgendas()

    return () => abortController.abort()
  }, [reloadAgendasAttempt])

  useEffect(() => {
    if (!queryPeriod || !selectedDoctor) {
      return
    }

    const { inicio, fim } = queryPeriod
    const abortController = new AbortController()

    async function loadAppointments() {
      setIsLoading(true)
      setLoadError(null)
      setAppointments([])

      try {
        const response = await consultarAgenda({
          profissionalUuid: selectedDoctor,
          inicio,
          fim,
          signal: abortController.signal,
        })

        if (!abortController.signal.aborted) {
          setAppointments(response.eventos.map(toFullCalendarEvent))
        }
      } catch (error) {
        if (abortController.signal.aborted) {
          return
        }

        setLoadError(
          error instanceof Error
            ? error.message
            : 'Não foi possível carregar os agendamentos.',
        )
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadAppointments()

    return () => abortController.abort()
  }, [queryPeriod, reloadAttempt, selectedDoctor])

  useEffect(() => {
    const calendarContainer = calendarContainerRef.current

    if (!calendarContainer) {
      return
    }

    let resizeFrame: number | undefined
    const mainContainer = calendarContainer.closest('main')

    if (!mainContainer) {
      return
    }

    const updateCalendarSize = () => {
      if (resizeFrame !== undefined) {
        cancelAnimationFrame(resizeFrame)
      }

      resizeFrame = requestAnimationFrame(() => {
        const mainStyles = getComputedStyle(mainContainer)
        const mainRect = mainContainer.getBoundingClientRect()
        const calendarRect = calendarContainer.getBoundingClientRect()
        const calendarOffset =
          calendarRect.top - mainRect.top + mainContainer.scrollTop
        const horizontalScrollbarHeight =
          calendarContainer.offsetHeight - calendarContainer.clientHeight
        const availableHeight =
          mainContainer.clientHeight -
          calendarOffset -
          (Number.parseFloat(mainStyles.paddingBottom) || 0) -
          horizontalScrollbarHeight
        const nextHeight = Math.max(
          MIN_CALENDAR_HEIGHT,
          Math.floor(availableHeight),
        )

        setCalendarHeight((currentHeight) =>
          currentHeight === nextHeight ? currentHeight : nextHeight,
        )
        calendarRef.current?.getApi().updateSize()
      })
    }

    const resizeObserver = new ResizeObserver(updateCalendarSize)

    resizeObserver.observe(mainContainer)
    updateCalendarSize()

    return () => {
      resizeObserver.disconnect()

      if (resizeFrame !== undefined) {
        cancelAnimationFrame(resizeFrame)
      }
    }
  }, [])

  function moveCalendar(direction: 'previous' | 'next') {
    const api = calendarRef.current?.getApi()

    if (direction === 'previous') {
      api?.prev()
    } else {
      api?.next()
    }
  }

  function goToToday() {
    const api = calendarRef.current?.getApi()
    api?.today()
    setSelectedDate(
      new Intl.DateTimeFormat('en-CA', {
        timeZone: 'America/Sao_Paulo',
      }).format(new Date()),
    )
  }

  function handleDateChange(event: ChangeEvent<HTMLInputElement>) {
    const nextDate = event.target.value

    if (!nextDate) {
      toast.warning('A data não pode ser nula.', {
        description: 'Selecione uma data válida para navegar na agenda.',
      })
      return
    }

    setSelectedDate(nextDate)
    calendarRef.current?.getApi().gotoDate(`${nextDate}T12:00:00`)
  }

  function changeCalendarView(view: CalendarView) {
    setCalendarView(view)
    calendarRef.current?.getApi().changeView(view)
  }

  function handleDatesSet(dateInfo: DatesSetArg) {
    setVisibleRange(formatVisiblePeriod(dateInfo))
    setQueryPeriod((currentPeriod) => {
      if (
        currentPeriod?.inicio === dateInfo.startStr &&
        currentPeriod.fim === dateInfo.endStr
      ) {
        return currentPeriod
      }

      return {
        inicio: dateInfo.startStr,
        fim: dateInfo.endStr,
      }
    })
  }

  return (
    <section
      aria-labelledby="agenda-title"
      className="flex min-w-0 flex-col gap-5"
    >
      <div className="flex flex-col justify-between gap-4 md:flex-row md:items-end">
        <div>
          <h1
            id="agenda-title"
            className="font-heading text-3xl font-bold tracking-[-0.02em] text-agend-ink"
          >
            Agenda Semanal
          </h1>
        </div>

        <button
          type="button"
          className="inline-flex min-h-10 cursor-not-allowed items-center justify-center gap-2 self-start rounded-lg bg-agend-brand-500 px-4 text-[13px] font-semibold text-white opacity-65 shadow-sm md:self-auto"
          title="Disponível em uma próxima etapa da PoC"
          disabled
        >
          <Plus aria-hidden="true" size={17} />
          Novo agendamento
        </button>
      </div>

      <div className="flex min-h-144 w-full min-w-0 flex-col overflow-hidden rounded-xl border border-agend-border bg-white shadow-agend-card">
        <div className="flex flex-col gap-4 border-b border-agend-border bg-white p-4 lg:flex-row lg:items-center lg:justify-between lg:px-6">
          <div className="flex min-w-0 flex-1 items-center gap-3">
            <div className="grid size-10 shrink-0 place-items-center rounded-lg bg-agend-brand-100 text-agend-brand-700">
              <Stethoscope aria-hidden="true" size={20} strokeWidth={1.8} />
            </div>
            <div className="min-w-0">
              <label
                htmlFor="doctor-filter"
                className="block text-[11px] font-semibold uppercase tracking-[0.08em] text-agend-subtle"
              >
                Agenda ativa
              </label>
              <select
                id="doctor-filter"
                value={selectedDoctor}
                onChange={(event) => setSelectedDoctor(event.target.value)}
                disabled={
                  isLoadingAgendas || Boolean(agendasError) || agendas.length === 0
                }
                aria-invalid={agendasError ? true : undefined}
                className="mt-0.5 max-w-full rounded-md border border-transparent bg-transparent py-1 pr-8 text-sm font-semibold text-agend-ink outline-none transition hover:border-agend-border focus:border-agend-brand-500 focus:ring-2 focus:ring-agend-brand-500/15 disabled:cursor-not-allowed disabled:text-agend-subtle"
              >
                {agendas.length === 0 ? (
                  <option value="">
                    {isLoadingAgendas
                      ? 'Carregando agendas...'
                      : agendasError
                        ? 'Agendas indisponíveis'
                        : 'Nenhuma agenda ativa'}
                  </option>
                ) : null}
                {agendas.map((agenda) => (
                  <option
                    key={agenda.profissionalUuid}
                    value={agenda.profissionalUuid}
                  >
                    {agenda.nomeExibicao}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <div
              className="flex min-h-9 items-center rounded-lg border border-agend-border bg-agend-canvas p-0.5"
              role="group"
              aria-label="Visualização da agenda"
            >
              {calendarViews.map(({ id, label, icon: Icon }) => {
                const isActive = calendarView === id

                return (
                  <button
                    key={id}
                    type="button"
                    onClick={() => changeCalendarView(id)}
                    className={`inline-flex min-h-8 items-center gap-1.5 rounded-md px-2.5 text-[11px] font-semibold transition focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-agend-brand-500 ${
                      isActive
                        ? 'bg-white text-agend-brand-700 shadow-sm'
                        : 'text-agend-muted hover:bg-white/70 hover:text-agend-brand-700'
                    }`}
                    aria-pressed={isActive}
                  >
                    <Icon aria-hidden="true" size={14} strokeWidth={1.8} />
                    <span className="hidden xl:inline">{label}</span>
                    <span className="sr-only xl:hidden">{label}</span>
                  </button>
                )
              })}
            </div>
            <label className="relative flex min-h-9 items-center rounded-lg border border-agend-border bg-white pl-9 pr-2 text-xs text-agend-muted transition focus-within:border-agend-brand-500 focus-within:ring-2 focus-within:ring-agend-brand-500/15">
              <span className="sr-only">Ir para uma data</span>
              <CalendarDays
                aria-hidden="true"
                className="absolute left-3 text-agend-brand-700"
                size={16}
              />
              <input
                type="date"
                value={selectedDate}
                onChange={handleDateChange}
                className="bg-transparent py-2 outline-none"
              />
            </label>
            <button
              type="button"
              onClick={goToToday}
              className="min-h-9 rounded-lg border border-agend-border bg-white px-3 text-xs font-semibold text-agend-ink transition hover:border-agend-brand-500 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
            >
              Hoje
            </button>
          </div>
        </div>

        <div className="flex min-h-16 flex-wrap items-center justify-between gap-3 border-b border-agend-border bg-agend-canvas/60 px-4 py-3 sm:px-6">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => moveCalendar('previous')}
              className="grid size-9 place-items-center rounded-lg border border-agend-border bg-white text-agend-muted transition hover:border-agend-brand-500 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
              aria-label="Período anterior"
            >
              <ChevronLeft aria-hidden="true" size={18} />
            </button>
            <button
              type="button"
              onClick={() => moveCalendar('next')}
              className="grid size-9 place-items-center rounded-lg border border-agend-border bg-white text-agend-muted transition hover:border-agend-brand-500 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
              aria-label="Próximo período"
            >
              <ChevronRight aria-hidden="true" size={18} />
            </button>
          </div>

          <div className="text-center">
            <p className="font-heading text-base font-semibold text-agend-ink sm:text-lg">
              {visibleRange}
            </p>
            <p className="text-[11px] text-agend-subtle">
              {isLoadingAgendas
                ? 'Carregando agendas ativas...'
                : agendasError
                  ? 'Agendas ativas indisponíveis'
                  : activeDoctor
                    ? `${activeDoctor.nomeExibicao} · ${
                        isLoading
                          ? 'Carregando atendimentos...'
                          : `${appointments.length} atendimentos`
                      }`
                    : 'Nenhuma agenda ativa disponível'}
            </p>
          </div>

          <span className="hidden min-w-18.5 justify-end text-[11px] font-medium text-agend-subtle sm:flex">
            UTC−3
          </span>
        </div>

        <div
          ref={calendarContainerRef}
          className="agenda-calendar relative w-full min-w-0 overflow-x-auto bg-white"
          aria-busy={isLoadingAgendas || isLoading}
        >
          {isLoadingAgendas || isLoading ? (
            <div
              className="absolute inset-0 z-20 flex items-center justify-center bg-white/70 backdrop-blur-[1px]"
              role="status"
            >
              <div className="flex items-center gap-2 rounded-lg border border-agend-border bg-white px-4 py-3 text-sm font-medium text-agend-muted shadow-agend-card">
                <LoaderCircle
                  aria-hidden="true"
                  className="animate-spin text-agend-brand-500"
                  size={18}
                />
                {isLoadingAgendas
                  ? 'Carregando agendas ativas...'
                  : 'Carregando agenda...'}
              </div>
            </div>
          ) : null}

          {!isLoadingAgendas && !isLoading && (agendasError || loadError) ? (
            <div
              className="absolute inset-x-4 top-4 z-20 flex flex-col items-start justify-between gap-3 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-800 shadow-sm sm:flex-row sm:items-center"
              role="alert"
            >
              <span className="flex items-start gap-2">
                <AlertCircle aria-hidden="true" className="mt-0.5" size={18} />
                {agendasError ?? loadError}
              </span>
              <button
                type="button"
                onClick={() =>
                  agendasError
                    ? setReloadAgendasAttempt((attempt) => attempt + 1)
                    : setReloadAttempt((attempt) => attempt + 1)
                }
                className="inline-flex min-h-9 shrink-0 items-center gap-2 rounded-lg border border-red-200 bg-white px-3 text-xs font-semibold text-red-800 transition hover:bg-red-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
              >
                <RefreshCw aria-hidden="true" size={15} />
                Tentar novamente
              </button>
            </div>
          ) : null}

          {!isLoadingAgendas &&
          !isLoading &&
          !agendasError &&
          !loadError &&
          agendas.length === 0 ? (
            <div
              className="pointer-events-none absolute inset-x-0 top-28 z-10 flex justify-center px-4"
              role="status"
            >
              <p className="rounded-lg border border-agend-border bg-white/95 px-4 py-3 text-sm text-agend-muted shadow-sm">
                Nenhuma agenda ativa disponível para esta clínica.
              </p>
            </div>
          ) : null}

          {!isLoadingAgendas &&
          !isLoading &&
          !agendasError &&
          !loadError &&
          agendas.length > 0 &&
          appointments.length === 0 ? (
            <div
              className="pointer-events-none absolute inset-x-0 top-28 z-10 flex justify-center px-4"
              role="status"
            >
              <p className="rounded-lg border border-agend-border bg-white/95 px-4 py-3 text-sm text-agend-muted shadow-sm">
                Nenhum agendamento encontrado neste período.
              </p>
            </div>
          ) : null}

          <FullCalendar
            ref={calendarRef}
            plugins={[timeGridPlugin, dayGridPlugin, luxonPlugin]}
            initialView="timeGridWeek"
            initialDate={new Date()}
            locale={ptBrLocale}
            timeZone="America/Sao_Paulo"
            firstDay={1}
            views={{
              dayGridMonth: {
                dayHeaderFormat: { weekday: 'short' },
              },
              timeGridWorkWeek: {
                type: 'timeGrid',
                duration: { weeks: 1 },
                dateAlignment: 'week',
                hiddenDays: [0, 6],
              },
            }}
            headerToolbar={false}
            allDaySlot={false}
            slotMinTime="07:00:00"
            slotMaxTime="20:00:00"
            slotDuration="00:30:00"
            slotLabelInterval="01:00:00"
            slotLabelFormat={{ hour: '2-digit', minute: '2-digit', hour12: false }}
            dayHeaderContent={CalendarDayHeader}
            events={appointments}
            eventContent={AppointmentCard}
            eventClassNames={['!border-0', '!bg-transparent', '!shadow-none']}
            datesSet={handleDatesSet}
            nowIndicator
            expandRows
            height={calendarHeight}
          />
        </div>
      </div>
    </section>
  )
}
