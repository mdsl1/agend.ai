import { useMemo, useRef, useState } from 'react'
import type { ChangeEvent } from 'react'
import type {
  DatesSetArg,
  DayHeaderContentArg,
  EventContentArg,
  EventInput,
} from '@fullcalendar/core'
import ptBrLocale from '@fullcalendar/core/locales/pt-br'
import dayGridPlugin from '@fullcalendar/daygrid'
import FullCalendar from '@fullcalendar/react'
import timeGridPlugin from '@fullcalendar/timegrid'
import { toast } from 'sonner'
import {
  CalendarDays,
  CalendarRange,
  ChevronLeft,
  ChevronRight,
  Columns3,
  Grid3X3,
  Plus,
  Stethoscope,
} from 'lucide-react'

type AppointmentDetails = {
  patient: string
  procedure: string
  doctorId: string
}

type CalendarView = 'timeGridWeek' | 'timeGridWorkWeek' | 'dayGridMonth'

const doctors = [
  { id: 'all', name: 'Todos os profissionais' },
  { id: 'ricardo', name: 'Dr. Ricardo Almeida' },
  { id: 'camila', name: 'Dra. Camila Santos' },
  { id: 'lucas', name: 'Dr. Lucas Oliveira' },
]

const calendarViews: Array<{
  id: CalendarView
  label: string
  icon: typeof CalendarRange
}> = [
  { id: 'timeGridWeek', label: 'Semana', icon: Columns3 },
  { id: 'timeGridWorkWeek', label: 'Semana útil', icon: CalendarRange },
  { id: 'dayGridMonth', label: 'Mês', icon: Grid3X3 },
]

const staticAppointments: EventInput[] = [
  {
    id: 'appointment-1',
    title: 'Ana Souza',
    start: '2026-07-27T08:00:00-03:00',
    end: '2026-07-27T09:00:00-03:00',
    extendedProps: {
      patient: 'Ana Souza',
      procedure: 'Consulta inicial',
      doctorId: 'ricardo',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-2',
    title: 'Carlos Mendes',
    start: '2026-07-27T10:30:00-03:00',
    end: '2026-07-27T11:30:00-03:00',
    extendedProps: {
      patient: 'Carlos Mendes',
      procedure: 'Retorno cardiológico',
      doctorId: 'camila',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-3',
    title: 'Fernanda Lima',
    start: '2026-07-28T09:00:00-03:00',
    end: '2026-07-28T10:00:00-03:00',
    extendedProps: {
      patient: 'Fernanda Lima',
      procedure: 'Avaliação dermatológica',
      doctorId: 'lucas',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-4',
    title: 'Rafael Costa',
    start: '2026-07-29T13:00:00-03:00',
    end: '2026-07-29T14:30:00-03:00',
    extendedProps: {
      patient: 'Rafael Costa',
      procedure: 'Consulta clínica',
      doctorId: 'ricardo',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-5',
    title: 'Juliana Rocha',
    start: '2026-07-30T08:30:00-03:00',
    end: '2026-07-30T09:30:00-03:00',
    extendedProps: {
      patient: 'Juliana Rocha',
      procedure: 'Retorno',
      doctorId: 'camila',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-6',
    title: 'Paulo Nunes',
    start: '2026-07-31T15:00:00-03:00',
    end: '2026-07-31T16:00:00-03:00',
    extendedProps: {
      patient: 'Paulo Nunes',
      procedure: 'Avaliação clínica',
      doctorId: 'ricardo',
    } satisfies AppointmentDetails,
  },
  {
    id: 'appointment-7',
    title: 'Beatriz Alves',
    start: '2026-08-01T10:00:00-03:00',
    end: '2026-08-01T11:00:00-03:00',
    extendedProps: {
      patient: 'Beatriz Alves',
      procedure: 'Consulta dermatológica',
      doctorId: 'lucas',
    } satisfies AppointmentDetails,
  },
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

function AppointmentCard({ event, view }: EventContentArg) {
  const details = event.extendedProps as AppointmentDetails
  const isMonthView = view.type === 'dayGridMonth'

  return (
    <div
      className={`h-full overflow-hidden rounded border-agend-brand-700 bg-agend-brand-100 text-agend-brand-700 shadow-sm ring-1 ring-agend-brand-500/10 ${
        isMonthView
          ? 'min-h-0 border-l-2 px-1.5 py-1'
          : 'min-h-12 border-l-4 px-2 py-1.5'
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
  const [selectedDoctor, setSelectedDoctor] = useState('all')
  const [selectedDate, setSelectedDate] = useState('2026-07-29')
  const [calendarView, setCalendarView] =
    useState<CalendarView>('timeGridWeek')
  const [visibleRange, setVisibleRange] = useState(
    '27 de julho — 2 de agosto de 2026',
  )

  const visibleAppointments = useMemo(() => {
    if (selectedDoctor === 'all') {
      return staticAppointments
    }

    return staticAppointments.filter(
      (appointment) => appointment.extendedProps?.doctorId === selectedDoctor,
    )
  }, [selectedDoctor])

  const activeDoctor = doctors.find((doctor) => doctor.id === selectedDoctor)

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
  }

  return (
    <section aria-labelledby="agenda-title" className="flex flex-col gap-5">
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

      <div className="overflow-hidden rounded-xl border border-agend-border bg-white shadow-agend-card">
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
                className="mt-0.5 max-w-full rounded-md border border-transparent bg-transparent py-1 pr-8 text-sm font-semibold text-agend-ink outline-none transition hover:border-agend-border focus:border-agend-brand-500 focus:ring-2 focus:ring-agend-brand-500/15"
              >
                {doctors.map((doctor) => (
                  <option key={doctor.id} value={doctor.id}>
                    {doctor.name}
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
              {activeDoctor?.name} · {visibleAppointments.length} atendimentos
            </p>
          </div>

          <span className="hidden min-w-18.5 justify-end text-[11px] font-medium text-agend-subtle sm:flex">
            UTC−3
          </span>
        </div>

        <div className="agenda-calendar w-full overflow-x-auto bg-white">
          <FullCalendar
            ref={calendarRef}
            plugins={[timeGridPlugin, dayGridPlugin]}
            initialView="timeGridWeek"
            initialDate="2026-07-29"
            locale={ptBrLocale}
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
            events={visibleAppointments}
            eventContent={AppointmentCard}
            eventClassNames={['!border-0', '!bg-transparent', '!shadow-none']}
            datesSet={handleDatesSet}
            nowIndicator
            expandRows
            height="auto"
          />
        </div>
      </div>
    </section>
  )
}
