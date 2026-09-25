import { useState, type FormEvent } from 'react'
import {
  CalendarCheck2,
  Eye,
  EyeOff,
  LoaderCircle,
  LockKeyhole,
  Mail,
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { LoginHero } from '../components/LoginHero'
import { appConfig } from '../config/appConfig'
import { ApiError } from '../features/api/apiClient'
import { useAuth } from '../features/auth/useAuth'

type LoginErrors = {
  email?: string
  password?: string
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validateEmail(email: string) {
  const normalizedEmail = email.trim()

  if (!normalizedEmail) {
    return 'Informe seu e-mail.'
  }

  if (!EMAIL_PATTERN.test(normalizedEmail)) {
    return 'Informe um e-mail válido.'
  }

  return undefined
}

function validatePassword(password: string) {
  if (!password) {
    return 'Informe sua senha.'
  }

  if (password.length < 8) {
    return 'A senha deve ter no mínimo 8 caracteres.'
  }

  return undefined
}

export function Login() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [errors, setErrors] = useState<LoginErrors>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const nextErrors = {
      email: validateEmail(email),
      password: validatePassword(password),
    }

    setErrors(nextErrors)

    if (nextErrors.email || nextErrors.password) {
      toast.warning('Revise os dados informados.', {
        description: 'Corrija os campos destacados para continuar.',
      })
      return
    }

    setIsSubmitting(true)

    try {
      await signIn({
        clinicaUuid: appConfig.pocClinicUuid,
        email: email.trim(),
        senha: password,
      })
      toast.success('Login realizado com sucesso.')
      navigate('/agenda', { replace: true })
    } catch (error) {
      const description =
        error instanceof ApiError && error.status === 401
          ? 'E-mail ou senha inválidos.'
          : error instanceof Error
            ? error.message
            : 'Tente novamente em alguns instantes.'

      toast.error('Não foi possível entrar.', { description })
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="relative flex min-h-screen w-full overflow-hidden bg-[linear-gradient(144.58deg,#e6f5f8_0%,#f8fafc_100%)] min-[821px]:min-w-[1024px]">
      <section className="relative z-10 flex min-h-screen w-full flex-col items-center gap-20 px-6 py-10 sm:px-12 min-[821px]:w-[600px] min-[821px]:shrink-0 min-[821px]:items-start min-[821px]:gap-[125px] min-[821px]:px-[60px] min-[821px]:py-[70px]">
        <div className="flex h-8 items-center gap-2 text-agend-brand-700">
          <CalendarCheck2 aria-hidden="true" size={32} strokeWidth={2.1} />
          <span className="font-heading text-2xl font-semibold leading-8">
            Agend.AI
          </span>
        </div>

        <div className="flex w-full max-w-[480px] flex-col items-center">
          <h1 className="mb-[60px] mt-10 w-full text-center font-heading text-[30px] font-bold leading-[38px] tracking-[-0.6px] text-agend-ink">
            Bem-vindo de volta!
          </h1>

          <form
            className="flex w-full flex-col gap-6"
            noValidate
            onSubmit={handleSubmit}
          >
            <div className="flex min-w-0 flex-col gap-1.5">
              <label
                className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                htmlFor="login-email"
              >
                E-mail
              </label>
              <div className="relative flex items-center">
                <Mail
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 text-agend-muted"
                  size={16}
                />
                <input
                  id="login-email"
                  type="email"
                  value={email}
                  autoComplete="username"
                  placeholder="seuemail@clinica.com"
                  aria-invalid={errors.email ? true : undefined}
                  aria-describedby={errors.email ? 'login-email-error' : undefined}
                  onBlur={() =>
                    setErrors((current) => ({
                      ...current,
                      email: validateEmail(email),
                    }))
                  }
                  onChange={(event) => {
                    setEmail(event.target.value)
                    if (errors.email) {
                      setErrors((current) => ({ ...current, email: undefined }))
                    }
                  }}
                  className="min-h-11 w-full rounded-lg border border-agend-border bg-white py-2.5 pl-10 pr-4 text-agend-ink outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 aria-invalid:border-red-500 aria-invalid:focus:border-red-500 aria-invalid:focus:ring-red-500/15"
                />
              </div>
              {errors.email ? (
                <p
                  id="login-email-error"
                  className="text-xs leading-[18px] text-red-600"
                  role="alert"
                >
                  {errors.email}
                </p>
              ) : null}
            </div>

            <div className="flex min-w-0 flex-col gap-1.5">
              <label
                className="text-[13px] font-semibold leading-4 tracking-[0.13px] text-agend-label"
                htmlFor="login-password"
              >
                Senha
              </label>
              <div className="relative flex items-center">
                <LockKeyhole
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 text-agend-muted"
                  size={16}
                />
                <input
                  id="login-password"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  autoComplete="current-password"
                  placeholder="Digite sua senha"
                  minLength={8}
                  aria-invalid={errors.password ? true : undefined}
                  aria-describedby={
                    errors.password ? 'login-password-error' : undefined
                  }
                  onBlur={() =>
                    setErrors((current) => ({
                      ...current,
                      password: validatePassword(password),
                    }))
                  }
                  onChange={(event) => {
                    setPassword(event.target.value)
                    if (errors.password) {
                      setErrors((current) => ({
                        ...current,
                        password: undefined,
                      }))
                    }
                  }}
                  className="min-h-11 w-full rounded-lg border border-agend-border bg-white py-2.5 pl-10 pr-11 text-agend-ink outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-agend-brand-500 focus:ring-3 focus:ring-agend-brand-500/15 aria-invalid:border-red-500 aria-invalid:focus:border-red-500 aria-invalid:focus:ring-red-500/15"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((isVisible) => !isVisible)}
                  className="absolute right-2.5 grid size-8 place-items-center rounded-md text-agend-muted transition hover:bg-agend-brand-100 hover:text-agend-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500"
                  aria-label={showPassword ? 'Ocultar senha' : 'Visualizar senha'}
                  aria-pressed={showPassword}
                >
                  {showPassword ? (
                    <EyeOff aria-hidden="true" size={17} />
                  ) : (
                    <Eye aria-hidden="true" size={17} />
                  )}
                </button>
              </div>
              {errors.password ? (
                <p
                  id="login-password-error"
                  className="text-xs leading-[18px] text-red-600"
                  role="alert"
                >
                  {errors.password}
                </p>
              ) : null}
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex min-h-12 w-full items-center justify-center gap-2 rounded-lg bg-agend-brand-500 px-4 text-[13px] font-semibold leading-4 text-white shadow-sm transition hover:bg-agend-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-agend-brand-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSubmitting ? (
                <LoaderCircle
                  aria-hidden="true"
                  className="animate-spin"
                  size={17}
                />
              ) : null}
              {isSubmitting ? 'Entrando...' : 'Entrar'}
            </button>
          </form>

          <p className="mt-12 w-full max-w-[448px] border-t border-agend-border pt-6 text-center text-xs leading-[18px] text-agend-muted">
            Problemas para acessar? Entre em contato com o administrador da sua
            clínica.
          </p>
        </div>
      </section>

      <LoginHero />
    </main>
  )
}
