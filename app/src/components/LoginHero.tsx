import loginHeroBackground from '../assets/login_hero_background_v2.png'

type HeroAppointment = {
  patient: string
  procedure: string
}

const heroAppointments: HeroAppointment[] = [
  { patient: 'Ana Martins', procedure: 'Primeira consulta' },
  { patient: 'Rafael Costa', procedure: 'Consulta de retorno' },
  { patient: 'Armelinda Silva', procedure: 'Exame de rotina' }
]

function HeroAppointmentCard({ patient, procedure }: HeroAppointment) {
  return (
    <div className="w-full max-w-[20rem] overflow-hidden rounded border-l-4 border-agend-brand-700 bg-agend-brand-100 px-3 py-2 text-agend-brand-700 shadow-[0_12px_30px_rgba(0,0,0,0.22)] ring-1 ring-inset ring-agend-brand-500/20">
      <p className="truncate text-[13px] font-semibold leading-4 text-agend-ink">
        {patient}
      </p>
      <p className="truncate text-[11px] leading-[14px] text-agend-muted">
        {procedure}
      </p>
    </div>
  )
}

export function LoginHero() {
  return (
    <section
      className="relative hidden h-screen min-h-[620px] flex-1 overflow-hidden bg-agend-brand-700 min-[821px]:block"
      aria-labelledby="login-hero-title"
    >
      <img
        src={loginHeroBackground}
        alt=""
        className="absolute inset-0 size-full object-cover object-center"
      />

      <div
        aria-hidden="true"
        className="absolute inset-0 bg-[linear-gradient(90deg,rgba(0,45,54,0.78)_0%,rgba(0,45,54,0.46)_48%,rgba(0,45,54,0.08)_100%)]"
      />
      <div
        aria-hidden="true"
        className="absolute inset-x-0 bottom-0 h-1/2 bg-[linear-gradient(0deg,rgba(9,22,30,0.74)_0%,rgba(9,22,30,0)_100%)]"
      />

      <div className="relative z-10 flex h-full flex-col justify-between gap-6 px-[clamp(2rem,4.5vw,4.5rem)] py-[clamp(2.5rem,7vh,5rem)] text-white">
        <h2
          id="login-hero-title"
          className="max-w-[34rem] font-heading text-[clamp(1.75rem,2.8vw,2.75rem)] font-semibold leading-[1.08] tracking-[-0.035em] [text-shadow:0_2px_18px_rgba(0,0,0,0.38)]"
        >
          Agendamento inteligente.
          <span className="mt-1 block text-white/88">
            Negócios mais rápidos.
          </span>
        </h2>

        <div
          className="my-auto flex w-full max-w-[26.25rem] flex-col py-2"
          aria-hidden="true"
        >
          <p className="mb-[1rem] text-[11px] font-bold uppercase tracking-[0.16em] text-white/88">
            Seus Agendamentos em Cards
          </p>
          <div className="-rotate-[1.5deg]">
            <HeroAppointmentCard {...heroAppointments[0]} />
          </div>
          <div className="-mt-2 ml-[clamp(1.5rem,4vw,4rem)] rotate-[1deg]">
            <HeroAppointmentCard {...heroAppointments[1]} />
          </div>
          <div className="-mt-1 ml-[clamp(1.5rem,4vw,4rem)] rotate-[-1deg]">
            <HeroAppointmentCard {...heroAppointments[2]} />
          </div>
        </div>

        <div className="max-w-[34rem] border-t border-white/30 pt-5 [text-shadow:0_1px_12px_rgba(0,0,0,0.45)]">
          <p className="mb-1.5 text-[11px] font-bold uppercase tracking-[0.16em] text-white/78">
            Ecossistema Agend.AI
          </p>
          <p className="font-heading text-[clamp(1rem,1.5vw,1.25rem)] font-medium leading-6 text-white">
            Website, aplicativo e chatbot conectados para sua clínica atender
            melhor em todos os canais.
          </p>
        </div>
      </div>
    </section>
  )
}
