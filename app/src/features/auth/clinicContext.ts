const CLINIC_UUID_STORAGE_KEY = 'agendai.auth.clinic-uuid.v1'

const UUID_PATTERN = /^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/i

export function normalizeClinicUuid(value: string | undefined) {
  const normalizedValue = value?.trim()

  return normalizedValue && UUID_PATTERN.test(normalizedValue)
    ? normalizedValue.toLowerCase()
    : null
}

export function getStoredClinicUuid() {
  return normalizeClinicUuid(
    window.localStorage.getItem(CLINIC_UUID_STORAGE_KEY) ?? undefined,
  )
}

export function saveClinicUuid(clinicUuid: string) {
  const normalizedClinicUuid = normalizeClinicUuid(clinicUuid)

  if (!normalizedClinicUuid) {
    throw new Error('O identificador da clínica informado é inválido.')
  }

  window.localStorage.setItem(CLINIC_UUID_STORAGE_KEY, normalizedClinicUuid)
}

export function getClinicLoginPath() {
  const clinicUuid = getStoredClinicUuid()

  return clinicUuid
    ? `/${clinicUuid}/login`
    : '/contexto-clinica-ausente'
}
