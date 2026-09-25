const pocClinicUuid = import.meta.env.VITE_POC_CLINIC_UUID

if (!pocClinicUuid) {
  throw new Error('A variável VITE_POC_CLINIC_UUID não foi configurada.')
}

export const appConfig = {
  pocClinicUuid,
} as const
